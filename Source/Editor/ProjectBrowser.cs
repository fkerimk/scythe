using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using ImGuiNET;
using Raylib_cs;
using static ImGuiNET.ImGui;

internal class ProjectBrowser : Viewport {

    private string
        _currentPath,
        _searchFilter = "";
    
    private const float
        ThumbnailSize = 64f,
        Padding = 16f;

    // Thumbnails
    private readonly Dictionary<string, Texture2D> _thumbnailCache = new();
    private readonly HashSet<string> _failedThumbnails = [], _pendingThumbnails = [];
    private readonly ConcurrentQueue<(string Path, Image Image)> _imageQueue = new();

    // Selection & Drag Logic
    private HashSet<string> _selectedPaths = [];
    private string? _selectionAnchor; // For Shift-Selection
    private bool _isBoxSelecting;
    private Vector2 _boxSelectStart;
    private HashSet<string> _preBoxSelection = []; // Selection before box drag started
    
    // Interaction Flags
    private bool _ignoreMouseRelease; // To prevent click-after-drag
    private bool _itemClickedThisFrame;

    // Rename State
    private string? _renamingPath;
    private string _renameBuffer = "";
    private bool _requestRenameFocus;
    private bool _setRenameSelection;

    // Search
    private string _lastSearch = "";
    private readonly List<string> _cachedSearchResults = [];

    public ProjectBrowser() : base("Project") {
        
        _currentPath = Config.Mod.Path;
        
        if (!Directory.Exists(_currentPath)) Directory.CreateDirectory(_currentPath);
        
        // Ensure Icons
        if (!Directory.Exists(PathUtil.ExeRelative("Temp/Trash"))) Directory.CreateDirectory(PathUtil.ExeRelative("Temp/Trash"));
        
        // Settings
        GetIO().MouseDoubleClickTime = 0.5f; // Windows standard
    }

    protected override void OnDraw() {
        
        // Process Loaded Thumbnails
        while (_imageQueue.TryDequeue(out var item)) {
            
             var tex = Raylib.LoadTextureFromImage(item.Image);
             Raylib.UnloadImage(item.Image); // Free CPU memory
             
             if (tex.Id != 0) {
                 
                 Raylib.SetTextureFilter(tex, TextureFilter.Bilinear);
                 _thumbnailCache[item.Path] = tex;
             }
             
             _pendingThumbnails.Remove(item.Path);
        }
        
        // Top Bar: Navigation -> Path -> Spacer -> Search (Right)
        PushFont(Fonts.ImFontAwesomeNormal);
        
        if (Button(Icons.FaLevelUp)) {
             var parent = Directory.GetParent(_currentPath);
             if (parent != null) _currentPath = parent.FullName;
        }
        
        PopFont();
        if (IsItemHovered()) SetTooltip("Up");

        SameLine();
        if (Button("Home")) _currentPath = Config.Mod.Path;

        SameLine();
        TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), GetUnixPath(_currentPath));

        // Right Align Search
        var avail = GetContentRegionAvail().X;
        const float searchWidth = 200f;
        
        var offset = avail - searchWidth;
        if (offset > 0) SameLine(GetCursorPosX() + offset);
        else SameLine(); 

        // Search Icon
        PushFont(Fonts.ImFontAwesomeSmall);
        SetCursorPosY(GetCursorPosY() + 4f); // Center vertically approx
        Text(Icons.FaSearch);
        PopFont();
        SameLine();
        
        SetNextItemWidth(searchWidth - 45f); // Adjust for icon and X button
        if (InputTextWithHint("##search", "Search...", ref _searchFilter, 64))
            UpdateSearch();
        
        // Clear Button
        if (!string.IsNullOrEmpty(_searchFilter)) {
            
            SameLine();
            
            if (Button("X")) {
                
                _searchFilter = "";
                UpdateSearch();
            }
        }
        
        Separator();

        // Split View: Tree | Content
        if (!BeginTable("ProjectBrowserLayout", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV)) return;
        TableSetupColumn("Tree", ImGuiTableColumnFlags.WidthFixed, 200f);
        TableSetupColumn("Files", ImGuiTableColumnFlags.WidthStretch);
            
        // Left Panel: Directory Tree
        TableNextColumn();
        BeginChild("TreeRegion");
        DrawDirectoryTree(Config.Mod.Path);
        EndChild();

        // Right Panel: Grid Content
        TableNextColumn();
        BeginChild("ContentRegion");
        DrawContentGrid(_currentPath);
        EndChild();

        EndTable();
    }
    
    private void UpdateSearch() {
        
        if (_searchFilter == _lastSearch) return;
        
        _lastSearch = _searchFilter;
        _cachedSearchResults.Clear();
        
        if (string.IsNullOrWhiteSpace(_searchFilter)) return;
        
        try {
            
            var allFiles = Directory.EnumerateFileSystemEntries(_currentPath, "*", SearchOption.AllDirectories);
            
            foreach (var f in allFiles) {
                
                if (!f.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase)) continue;
                
                _cachedSearchResults.Add(f);
                
                if (_cachedSearchResults.Count > 500) break; // Limit
            }
            
        } catch {
            
            // ignored
        }
    }
    
    private void DrawDirectoryTree(string rootPath) {
        
        if (!Directory.Exists(rootPath)) return;

        var name = Path.GetFileName(rootPath);
        if (string.IsNullOrEmpty(name)) name = rootPath;

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (_currentPath == rootPath) flags |= ImGuiTreeNodeFlags.Selected;

        var hasSubdirs = false;
        try { hasSubdirs = Directory.EnumerateDirectories(rootPath).Any(); }
        catch {
            // ignored
        }

        if (!hasSubdirs) flags |= ImGuiTreeNodeFlags.Leaf;

        var isNodeOpen = TreeNodeEx(rootPath, flags, name);
        
        // Drop Target (Tree)
        HandleDropTarget(rootPath);

        if (IsItemClicked() && !IsItemToggledOpen())
            _currentPath = rootPath;

        if (!isNodeOpen) return;
        
        if (hasSubdirs)
            foreach (var dir in Directory.GetDirectories(rootPath))
                DrawDirectoryTree(dir);
        
        TreePop();
    }

    private void DrawContentGrid(string path) {
        
        if (!Directory.Exists(path)) return;
        
        _itemClickedThisFrame = false;
        _ignoreMouseRelease = false;

        // Handle Box Selection State
        if (_isBoxSelecting) {
            
            if (!IsMouseDown(ImGuiMouseButton.Left)) {
                
                _isBoxSelecting = false;
                _ignoreMouseRelease = true; 
                _preBoxSelection.Clear();
                
            }
            
            else _selectedPaths = [.._preBoxSelection];
        }
        
        var avail = GetContentRegionAvail();
        var availWidth = avail.X;

        // Calculate Box Rect for Intersection Logic
        var boxRect = new Rect();
        
        if (_isBoxSelecting) {
            
             var mousePos = GetMousePos();
             var min = new Vector2(Math.Min(_boxSelectStart.X, mousePos.X), Math.Min(_boxSelectStart.Y, mousePos.Y));
             var max = new Vector2(Math.Max(_boxSelectStart.X, mousePos.X), Math.Max(_boxSelectStart.Y, mousePos.Y));
             boxRect = new Rect(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        // Draw Background Button (Capture empty space clicks)
        SetNextItemAllowOverlap();
        InvisibleButton("##grid_bg", avail);
        var bgClicked = IsItemClicked(ImGuiMouseButton.Left) && !_itemClickedThisFrame;
        
        // Background Context Menu
        if (BeginPopupContextItem("GridCMS", ImGuiPopupFlags.MouseButtonRight)) {
            
            if (MenuItem("Create New Text File"))
                CreateNewTextFile();
            
            EndPopup();
        }
        
        // Start Box Selection
        if (IsItemActive() && IsMouseDragging(ImGuiMouseButton.Left)) {
            
             if (!_isBoxSelecting) {
                 
                 _isBoxSelecting = true;
                 _boxSelectStart = GetMousePos();
                 _preBoxSelection = new HashSet<string>(_selectedPaths);
                 
                 if (!GetIO().KeyCtrl) {
                     
                     _selectedPaths.Clear();
                     _preBoxSelection.Clear();
                 }
             }
        }
        
        // Reset Cursor to draw items on top
        SetCursorScreenPos(GetItemRectMin());
        
        // Content
        IEnumerable<string> entries = string.IsNullOrWhiteSpace(_searchFilter)
            ? Directory.GetFileSystemEntries(_currentPath)
            : _cachedSearchResults;

        // Sort
        var entriesList = entries
            .OrderByDescending(Directory.Exists) 
            .ThenBy(Path.GetFileName, new NaturalStringComparer()!)
            .ToList();

        var columns = (int)(availWidth / (ThumbnailSize + Padding));
        if (columns < 1) columns = 1;

        Columns(columns, "Grid", false);
        
        var searchActive = !string.IsNullOrWhiteSpace(_searchFilter);
        
        foreach (var entryPath in entriesList) {
            
            var isDirectory = Directory.Exists(entryPath);

            if (DrawGridItem(entryPath, isDirectory, boxRect, out var doubleClicked)) {
                
                // Shift Selection
                if (GetIO().KeyShift) HandleShiftSelection(entryPath, entriesList);

                // Double Click Nav
                if (searchActive && doubleClicked) {
                    
                    var parent = Path.GetDirectoryName(entryPath);
                    
                    if (parent != null && Directory.Exists(parent)) {
                        
                        _currentPath = parent;
                        _searchFilter = "";
                        _lastSearch = "";
                    }
                }
            }
            
            NextColumn();
        }
        
        Columns(1);
        
        // Clear Selection if BG Clicked
        if (bgClicked && !_itemClickedThisFrame && !_isBoxSelecting && !_ignoreMouseRelease && !GetIO().KeyCtrl)
             _selectedPaths.Clear();

        // Draw Box Overlay
        if (_isBoxSelecting) {
            
             var drawList = GetWindowDrawList();
             drawList.AddRectFilled(boxRect.Min, boxRect.Max, GetColorU32(ImGuiCol.TextSelectedBg, 0.2f));
             drawList.AddRect(boxRect.Min, boxRect.Max, GetColorU32(ImGuiCol.TextSelectedBg, 0.8f));
        }

        // Keyboard Shortcuts
        if (IsWindowFocused()) {
            
            if (IsKeyPressed(ImGuiKey.Delete) && _selectedPaths.Count > 0) DeleteSelectedItems();
            if (IsKeyPressed(ImGuiKey.F2) && _selectedPaths.Count == 1) StartRename(_selectedPaths.First());
        }
    }
        


    private bool DrawGridItem(string path, bool isDirectory, Rect selectBox, out bool doubleClicked) {
        
        var itemClicked = false;
        doubleClicked = false;

        var name = Path.GetFileName(path);
        
        // Truncate logic for safety
        var displayName = name;
        if (displayName.Length > 24) displayName = displayName.Substring(0, 21) + "...";

        PushID(path);
        
        var drawList = GetWindowDrawList();
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(1);
        
        const float padding = 4f;
        
        var cellStartScreen = GetCursorScreenPos(); 
        var contentStartScreen = cellStartScreen + new Vector2(padding, padding);
        SetCursorScreenPos(contentStartScreen);

        BeginGroup();
        
        var groupStartX = GetCursorPosX();

        // Calculate Icon Size Reference (Standardize to Font Height)
        PushFont(Fonts.ImFontAwesomeLarge);
        var standardIconSize = CalcTextSize(isDirectory ? Icons.FaFolder : Icons.FaFile);
        var maxDim = Math.Max(standardIconSize.X, standardIconSize.Y);
        
        // Thumbnail or Icon
        var texId = GetThumbnail(path);
        
        if (texId != IntPtr.Zero) {
            
             var tex = _thumbnailCache[path];
             float w = tex.Width;
             float h = tex.Height;
             
             // Scale to fit within the standard Icon Size
             var ratio = w / h;
             var drawW = maxDim;
             var drawH = maxDim;
             
             if (w > h) drawH = drawW / ratio;
             else drawW = drawH * ratio;

             // Center in the Cell
             var imgOffsetX = (ThumbnailSize - drawW) * 0.5f;
             if (imgOffsetX < 0) imgOffsetX = 0;
             
             SetCursorPosX(groupStartX + imgOffsetX);
             Image((IntPtr)tex.Id, new Vector2(drawW, drawH));
             
        } else {
            
            // Font Icon Fallback
            var fallbackIcon = isDirectory ? Icons.FaFolder : Icons.FaFile;
            
            // Simple Extension Mapping for better generic look (Linux/Fallback)
            if (!isDirectory) {
                
                fallbackIcon = Icons.FaFile;
                
                //var ext = Path.GetExtension(path).ToLower();
                //
                //fallbackIcon = ext switch {
                //    
                //    ".cs" or ".json" or ".xml" or ".shader" or ".lua" or ".js" => Icons.FaCode,
                //    ".txt" or ".md" => Icons.FaFile,
                //    
                //    _ => fallbackIcon
                //};
            }

            var iconSize = CalcTextSize(fallbackIcon);
            
            var iconOffset = (ThumbnailSize - iconSize.X) * 0.5f;
            if (iconOffset < 0) iconOffset = 0;
            SetCursorPosX(groupStartX + iconOffset);
            
            if (isDirectory) PushStyleColor(ImGuiCol.Text, new Vector4(1f, 0.8f, 0.2f, 1f));
            Text(fallbackIcon);
            if (isDirectory) PopStyleColor();
        }

        PopFont();

        // Text
        if (_renamingPath == path) {
            
            // Rename Input
            SetNextItemWidth(ThumbnailSize);
            SetCursorPosX(groupStartX);
            
            if (_requestRenameFocus) {
                
                SetKeyboardFocusHere();
                _requestRenameFocus = false;
            }

            // Callback to handle initial selection (exclude extension)
            unsafe {
                
                int Callback(ImGuiInputTextCallbackData* data) {
                    
                    if (!_setRenameSelection) return 0;
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(_renameBuffer);
                    data->SelectionStart = 0;
                    data->SelectionEnd = nameWithoutExt.Length;
                    data->CursorPos = 0;
                    _setRenameSelection = false;
                    return 0;
                }

                if (InputText("##rename", ref _renameBuffer, 128, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways, Callback))
                    ConfirmRename();
            }
            
            // Cancel on Escape
            if (IsItemActive() && IsKeyPressed(ImGuiKey.Escape))
                CancelRename();
            
            // Cancel on clicking away
            if (IsItemDeactivated())
                 if (_renamingPath != null) CancelRename();
        }
        else {
            
            var textSize = CalcTextSize(displayName);
            
            if (textSize.X <= ThumbnailSize) {
                
                var textOffset = (ThumbnailSize - textSize.X) * 0.5f;
                SetCursorPosX(groupStartX + textOffset);
                Text(displayName);
                
            } else {
                
                SetCursorPosX(groupStartX);
                PushTextWrapPos(groupStartX + ThumbnailSize);
                TextWrapped(displayName);
                PopTextWrapPos();
            }
        }
        
        EndGroup();
        
        var groupHeight = GetItemRectSize().Y;
        var boxMin = cellStartScreen; 
        var boxMax = new Vector2(cellStartScreen.X + ThumbnailSize + 2 * padding, cellStartScreen.Y + groupHeight + 2 * padding);
        var itemRect = new Rect(boxMin.X, boxMin.Y, boxMax.X - boxMin.X, boxMax.Y - boxMin.Y);

        // Interaction
        SetCursorScreenPos(boxMin);
        InvisibleButton("btn", boxMax - boxMin);
        
        var hovered = IsItemHovered();
        var isSelected = _selectedPaths.Contains(path);

        // Selection Logic via Click
        
        if (IsItemClicked(ImGuiMouseButton.Left)) {
            
            if (!_isBoxSelecting) {
                
                _itemClickedThisFrame = true;
                itemClicked = true;
                
                // Skip local selection logic and let the Caller (DrawContentGrid) handle Range Selection.
                if (!GetIO().KeyShift) {
                    
                    _selectionAnchor = path; // Update Anchor
                      
                     // Ctrl Click
                    if (GetIO().KeyCtrl) {
                        
                        if (isSelected)
                             _selectedPaths.Remove(path);
                        else _selectedPaths.Add(path);
                        
                    } else {
                        
                        // If NOT already selected, select immediately (and clear others)
                        if (!isSelected) {
                            
                            _selectedPaths.Clear();
                            _selectedPaths.Add(path);
                        }
                        
                        // If ALREADY selected, we wait for Release (handled below)
                    }
                }
            }
        }
        
        // Handle "Click on Selected" -> Deselect others on Release if NOT Dragged
        if (IsItemHovered() && IsMouseReleased(ImGuiMouseButton.Left)) {
            
             if (!_isBoxSelecting && !GetIO().KeyCtrl && !GetIO().KeyShift && isSelected) {
                 
                 // Check if it was a drag
                 if (GetMouseDragDelta(ImGuiMouseButton.Left).Length() < 2.0f) {
                     
                      _selectedPaths.Clear();
                      _selectedPaths.Add(path);
                 }
             }
        }
        
        if (IsItemHovered() && IsMouseDoubleClicked(ImGuiMouseButton.Left)) {
            
            doubleClicked = true;
            
            if (isDirectory) { // Immediate navigation for directories
                
                _itemClickedThisFrame = true;
                _currentPath = path;
                _selectedPaths.Clear();
            }
        }

        // Context Menu
        if (BeginPopupContextItem($"ItemCMS_{path}")) {
            
            // Select on right click if not selected
            if (!_selectedPaths.Contains(path)) {
                
                _selectedPaths.Clear();
                _selectedPaths.Add(path);
            }
            
            if (MenuItem("Delete")) DeleteSelectedItems();
            if (MenuItem("Rename")) StartRename(path);
            
            EndPopup();
        }
        
        if (IsItemClicked(ImGuiMouseButton.Right))
            _itemClickedThisFrame = true;
        
        // Drag Source
        if (BeginDragDropSource()) {
            
            // If dragging unselected item, select it (exclusive)
            if (!_selectedPaths.Contains(path)) {
                
                _selectedPaths.Clear();
                _selectedPaths.Add(path);
            }
            
            // Mark as interacting so BG doesn't clear
            _itemClickedThisFrame = true; 
            
            var files = string.Join("|", _selectedPaths);
            SetDragDropPayload("MOVE_FILES", IntPtr.Zero, 0); 

            DragDropPayload.Data = files; // Use a helper static
            
            Text($"{_selectedPaths.Count} items");
            EndDragDropSource();
        }

        // Drop Target
        if (isDirectory) HandleDropTarget(path);

        // Box Selection Logic
        if (_isBoxSelecting && itemRect.Intersects(selectBox)) {
            
            _selectedPaths.Add(path);
            isSelected = true; // Visual Update
        }

        // Draw Background
        drawList.ChannelsSetCurrent(0);
        
        if (isSelected)
            drawList.AddRectFilled(boxMin, boxMax, GetColorU32(ImGuiCol.ButtonActive), 4f);
        else if (hovered)
            drawList.AddRectFilled(boxMin, boxMax, GetColorU32(ImGuiCol.HeaderHovered), 4f);

        drawList.ChannelsMerge();

        PopID();

        return itemClicked;
    }
    
    private unsafe void HandleDropTarget(string targetPath) {
        
        if (!BeginDragDropTarget()) return;
        
        var payload = AcceptDragDropPayload("MOVE_FILES");
        
        // Execute Move
        if (payload.NativePtr != null)
            MoveSelectedFiles(targetPath);
        
        EndDragDropTarget();
    }
    
    private void MoveSelectedFiles(string targetDir) {

        // Verify we are moving _selectedPaths
        var validPaths = _selectedPaths.ToList(); // Copy
        _selectedPaths.Clear();
        
        var moves = new List<(string Src, string Dest)>();

        // Calculate Moves
        foreach (var src in validPaths) {
            
            if (src == targetDir) continue; 
            var name = Path.GetFileName(src);
            var dest = Path.Combine(targetDir, name);
            
            // Check strict validity
            if (src == dest) continue;
            if (File.Exists(dest) || Directory.Exists(dest)) continue; // Skip collisions
            
            moves.Add((src, dest));
        }
        
        if (moves.Count == 0) return;

        // Perform & Record
        History.StartRecording(this, $"Move {moves.Count} items");

        PerformMove(moves);

        History.SetUndoAction(() => PerformMove(moves.Select(m => (m.Dest, m.Src))));
        History.SetRedoAction(() => PerformMove(moves));
        
        History.StopRecording();
        return;

        void PerformMove(IEnumerable<(string s, string d)> items) {
            
            foreach (var (s, d) in items) {
                
                try {
                    
                    if (Directory.Exists(s)) Directory.Move(s, d);
                    else if (File.Exists(s)) File.Move(s, d);
                    
                } catch (Exception e) { Console.WriteLine($"Move error: {e.Message}"); }
            }
        }
    }
    
    private void DeleteSelectedItems() {
        
        var pathsToDelete = _selectedPaths.ToList();
        _selectedPaths.Clear();
        
        if (pathsToDelete.Count == 0) return;

        // Ensure trash dir exists
        var trashDir = PathUtil.ExeRelative("Temp/Trash");
        Directory.CreateDirectory(trashDir);

        var backups = new List<(string Original, string Backup, bool IsDir)>();

        // Backup
        foreach (var path in pathsToDelete) {
            
             if (!File.Exists(path) && !Directory.Exists(path)) continue;
             
             var isDir = Directory.Exists(path);
             var backupName = Guid.NewGuid().ToString();
             var backupPath = Path.Combine(trashDir, backupName);
             
             try {
                 
                 if (isDir) CopyDirectory(path, backupPath);
                 else File.Copy(path, backupPath);
                 
                 backups.Add((path, backupPath, isDir));
                 
             }
             
             catch (Exception e) { Console.WriteLine($"Backup failed for {path}: {e.Message}"); }
        }

        // Perform & Record
        History.StartRecording(this, $"Delete {backups.Count} items");
        
        RedoDelete(); // Do
        
        History.SetUndoAction(UndoDelete);
        History.SetRedoAction(RedoDelete);
        
        History.StopRecording();
        
        return;

        // Define Actions
        void UndoDelete() {
            
            foreach (var b in backups) {
                
                try {
                    
                    if (b.IsDir && !Directory.Exists(b.Original))
                        if (Directory.Exists(b.Backup)) CopyDirectory(b.Backup, b.Original);
                        
                    else if (!b.IsDir && !File.Exists(b.Original))
                        if (File.Exists(b.Backup)) File.Copy(b.Backup, b.Original);
                    
                }
                
                catch { /**/ }
            }
        }
        
        void RedoDelete() {
            
            foreach (var b in backups)
                RecyclePath(b.Original);
        }

    }
    
    private static void CopyDirectory(string sourceDir, string destinationDir) {
        
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) return;

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles()) {
            
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (var subDir in dir.GetDirectories()) {
            
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    private static void RecyclePath(string path) {
        
        try {
            
            if (OperatingSystem.IsWindows()) {

                var psCommand = $"Add-Type -AssemblyName Microsoft.VisualBasic; [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteFile('{path}', 'OnlyErrorDialogs', 'SendToRecycleBin')";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -Command \"{psCommand}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            } 
            else {

                try { System.Diagnostics.Process.Start("gio", $"trash \"{path}\""); }
                
                catch {

                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    else File.Delete(path);
                }
            }
        }
        
        catch (Exception e) { Console.WriteLine($"Error deleting {path}: {e.Message}"); }
    }

    private void CreateNewTextFile() {
        
        const string baseName = "New Text Document";
        const string extension = ".txt";
        
        var fileName = baseName + extension;
        var fullPath = Path.Combine(_currentPath, fileName);
        
        var counter = 1;
        
        while (File.Exists(fullPath) || Directory.Exists(fullPath)) {
            
            fileName = $"{baseName} ({counter}){extension}";
            fullPath = Path.Combine(_currentPath, fileName);
            counter++;
        }
        
        try { File.WriteAllText(fullPath, ""); }
        catch (Exception e) { Console.WriteLine($"Error creating file: {e.Message}"); }
    }
    
    // Rename Helpers
    public void RenameSelected() {
        
        if (_selectedPaths.Count == 1)
            StartRename(_selectedPaths.First());
    }

    private void HandleShiftSelection(string targetPath, List<string> entries) {
        
        if (_selectionAnchor == null || !entries.Contains(_selectionAnchor)) {
            
            _selectionAnchor = targetPath;
            _selectedPaths.Clear();
            _selectedPaths.Add(targetPath);
            return;
        }
        
        var startIdx = entries.IndexOf(_selectionAnchor);
        var endIdx = entries.IndexOf(targetPath);
        
        if (startIdx == -1 || endIdx == -1) return;
        
        var min = Math.Min(startIdx, endIdx);
        var max = Math.Max(startIdx, endIdx);
        
        if (!GetIO().KeyCtrl) _selectedPaths.Clear();
        
        for (var i = min; i <= max; i++) _selectedPaths.Add(entries[i]);
    }
    
    private void StartRename(string path) {
        
        _renamingPath = path;
        _renameBuffer = Path.GetFileName(path);
        _requestRenameFocus = true;
        _setRenameSelection = true;
    }

    private void ConfirmRename() {
        
        if (string.IsNullOrEmpty(_renamingPath)) return;
        
        var newName = _renameBuffer;
        if (string.IsNullOrWhiteSpace(newName)) { CancelRename(); return; }

        var dir = Path.GetDirectoryName(_renamingPath);
        if (dir == null) return;
        var oldName = Path.GetFileName(_renamingPath);
        
        if (newName == oldName) { CancelRename(); return; }
        
        var newPath = Path.Combine(dir, newName);
        
        // Final Path check
        if (Directory.Exists(newPath) || File.Exists(newPath)) {
            
             Console.WriteLine("Rename failed: Destination exists.");
             _renamingPath = null;
             return;
        }
        
        var oldPath = _renamingPath;

        // History
        History.StartRecording(this, $"Rename {oldName}");

        DoMove(oldPath, newPath);
        
        History.SetUndoAction(() => DoMove(newPath, oldPath));
        History.SetRedoAction(() => DoMove(oldPath, newPath));

        History.StopRecording();

        // Update selection if it was selected
        if (_selectedPaths.Contains(oldPath)) {
            
            _selectedPaths.Remove(oldPath);
            _selectedPaths.Add(newPath);
        }
        
        _renamingPath = null;
        return;

        // Action
        void DoMove(string s, string d) {
            
            try {
                
                if (Directory.Exists(s)) Directory.Move(s, d);
                else if (File.Exists(s)) File.Move(s, d);
                
            } catch { /**/ }
        }
    }

    private void CancelRename() => _renamingPath = null;

    // Thumbnails
    private IntPtr GetThumbnail(string path) {
        
        if (_failedThumbnails.Contains(path)) return IntPtr.Zero;
        if (_thumbnailCache.TryGetValue(path, out var tex)) return (IntPtr)tex.Id;
        
        if (_pendingThumbnails.Contains(path)) return IntPtr.Zero;

        // Check for Image Extension
        var ext = Path.GetExtension(path).ToLower();
        var imgExts = new HashSet<string> { ".png", ".jpg", ".jpeg", ".bmp", ".tga", ".gif", ".psd", ".hdr", ".qoi" };
         
        if (imgExts.Contains(ext)) {
            
            _pendingThumbnails.Add(path);
            
            Task.Run(() => {
                
                unsafe {
                    
                    if (!File.Exists(path)) return;
                    
                    var img = Raylib.LoadImage(path);

                    if (img.Data == null) return;
                    
                    const int maxDim = 256;
                    var w = img.Width;
                    var h = img.Height;
                        
                    if (w > maxDim || h > maxDim) {
                            
                        if (w > h) {
                                
                            h = (int)((float)h / w * maxDim);
                            w = maxDim;
                                
                        } else {
                                
                            w = (int)((float)w / h * maxDim);
                            h = maxDim;
                        }
                            
                        Raylib.ImageResize(ref img, w, h);
                    }
                        
                    _imageQueue.Enqueue((path, img));

                    // Failed
                }
            });
            
            return IntPtr.Zero;
        }

        // Try Load Synchronous (System Icons)
        var t = LoadThumbnail(path);
        
        if (t.Id != 0) {
            
            _thumbnailCache[path] = t;
            return (IntPtr)t.Id;
        }

        _failedThumbnails.Add(path);
        return IntPtr.Zero;
    }
    
    private Texture2D LoadThumbnail(string path) {
        
        // Windows System Thumbnail (Shell Icon)
        if (OperatingSystem.IsWindows())
            return TryLoadWindowsThumbnail(path);
        
        return new Texture2D { Id = 0 };
    }
    
    private static string GetUnixPath(string path) {
        
        if (string.IsNullOrEmpty(path)) return "/";
        
        var p = path.Replace('\\', '/');
        
        // Handle Drive Letter formatting: C:/ -> /c/
        if (p.Length < 2 || p[1] != ':') return p;
        
        var drive = char.ToLower(p[0]);
        var rest = p.Length > 2 ? p[2..] : "/";
        if (!rest.StartsWith('/')) rest = "/" + rest;
        return $"/{drive}{rest}";

    }

    private static unsafe Texture2D TryLoadWindowsThumbnail(string path) {
        
        try {
            
            var shinfo = new Win32.Shfileinfo();
            
            const uint flags = 0x000000100 | 0x000000000; // ICON | LARGEICON
            
            Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);
            var hIcon = shinfo.hIcon;
            
            if (hIcon != IntPtr.Zero) {
                
                const int size = 32; // Standard Large Icon
                
                // GDI Context to draw Icon to Bitmap
                var hDc = Win32.GetDC(IntPtr.Zero);
                var hMemDc = Win32.CreateCompatibleDC(hDc);
                
                var bi = new Win32.Bitmapinfo();
                bi.biHeader.biSize = (uint)Marshal.SizeOf<Win32.Bitmapinfoheader>();
                bi.biHeader.biWidth = size;
                bi.biHeader.biHeight = -size; // Top-down
                bi.biHeader.biPlanes = 1;
                bi.biHeader.biBitCount = 32;
                bi.biHeader.biCompression = 0; // BI_RGB

                var hBitmap = Win32.CreateDIBSection(hMemDc, ref bi, 0, out var pBits, IntPtr.Zero, 0);
                
                Win32.SelectObject(hMemDc, hBitmap);
                
                Win32.DrawIconEx(hMemDc, 0, 0, hIcon, size, size, 0, IntPtr.Zero, 0x0003); // DI_MASK | DI_IMAGE
                
                // We have bits at pBits (BGRA). Size = 32*32*4
                const int bufferSize = size * size * 4;
                
                // Swizzle BGRA -> RGBA
                var src = (byte*)pBits;
                var buffer = new byte[bufferSize];
                
                // Copy and Swizzle
                for(var i=0; i<size*size; i++) {
                    
                    var padding = i * 4;
                    var b = src[padding];
                    var g = src[padding+1];
                    var r = src[padding+2];
                    var a = src[padding+3];
                    
                    buffer[padding] = r;
                    buffer[padding+1] = g;
                    buffer[padding+2] = b;
                    buffer[padding+3] = a;
                }
                
                Win32.DeleteObject(hBitmap);
                Win32.DeleteDC(hMemDc);
                Win32.ReleaseDC(IntPtr.Zero, hDc);
                Win32.DestroyIcon(hIcon);

                // Create Raylib Texture
                fixed (byte* pData = buffer) {
                    
                    var img = new Image {
                     
                        Data = pData,
                        Width = size,
                        Height = size,
                        Mipmaps = 1,
                        Format = PixelFormat.UncompressedR8G8B8A8
                    };
                    
                    return Raylib.LoadTextureFromImage(img); 
                }
            }
        }
        
        catch { /**/ }
        
        return new Texture2D { Id = 0 };
    }

    // Helper struct for Rect
    public readonly struct Rect(float x, float y, float w, float h) {
        
        private readonly float _x = x, _y = y, _w = w, _h = h;
        public Vector2 Min => new(_x, _y);
        public Vector2 Max => new(_x + _w, _y + _h);
        public bool Intersects(Rect r) => _x < r._x + r._w && _x + _w > r._x && _y < r._y + r._h && _y + _h > r._y;
    }
}