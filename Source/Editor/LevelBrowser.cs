using System.Numerics;
using ImGuiNET;
using static ImGuiNET.ImGui;

internal class LevelBrowser() : Viewport("Level") {

    // cache
    private float? _savedScroll;

    private static Obj?
        _dragObject,
        _dragTarget,
        _scheduledDeleteObject;
    
    public static Obj? SelectedObject { get; private set; }

    private int _rowCount;
    private readonly float[] _lastRowY = new float[128];
    
    protected override void OnDraw() {

        if (Core.ActiveLevel == null) return;
        
        _rowCount = 0;
        Array.Clear(_lastRowY, 0, _lastRowY.Length);

        BeginChild("scroll", new Vector2(0, 0));
        
        // restore scroll 
        if (_savedScroll != null) {
            
            SetScrollY(_savedScroll.Value);
            _savedScroll = null;
        }

        // drag object
        if (_dragObject != null && _dragTarget != null) {

            _dragObject.SetParent(_dragTarget);
            
            _dragObject = null;
            _dragTarget = null;
        }

        // Delete object
        if (_scheduledDeleteObject != null) {

            if (_scheduledDeleteObject != Core.ActiveLevel.Root) {
                
                _scheduledDeleteObject.RecordedDelete();
            }
            
            _scheduledDeleteObject = null;
        }
        
        // draw objects
        DrawObject(Core.ActiveLevel.Root);
        
        EndChild();
    }

    private static bool IsAncestorOf(Obj ancestor, Obj target) {

        var current = target.Parent;

        while (current != null) {
            
            if (current == ancestor) return true;
            current = current.Parent;
        }
        
        return false;
    }

    private bool DrawObject(Obj obj, int indent = 0) {

        if (Core.ActiveLevel == null) return true;

        var drawList = GetWindowDrawList();
        var rowPos = GetCursorScreenPos();
        var rowHeight = GetFrameHeight();
        var centerY = rowPos.Y + rowHeight / 2f;
        
        // Zebra stripping
        if (_rowCount % 2 == 0) {
            
            drawList.AddRectFilled(
                
                new Vector2(GetWindowPos().X, rowPos.Y),
                new Vector2(GetWindowPos().X + GetWindowWidth(), rowPos.Y + rowHeight),
                GetColorU32(new Vector4(1, 1, 1, 0.012f))
            );
        }
        _rowCount++;

        // Hierarchy lines
        var lineCol = GetColorU32(new Vector4(1, 1, 1, 0.15f));
        
        if (indent > 0) {
            
            var indentStep = GetStyle().IndentSpacing; 
            var lineX = rowPos.X - indentStep + 10f;
            
            // Horizontal line
            drawList.AddLine(new Vector2(lineX, centerY), new Vector2(lineX + 11f, centerY), lineCol);
            
            // Vertical line
            if (_lastRowY[indent] > 0)
                drawList.AddLine(new Vector2(lineX, _lastRowY[indent]), new Vector2(lineX, centerY), lineCol);
        }
        _lastRowY[indent] = centerY;

        if (SelectedObject != null && IsAncestorOf(obj, SelectedObject)) {
            
            SetNextItemOpen(true);
        }
        
        // tree node
        var id = "##" + obj.GetHashCode();
        var isOpen = GetStateStorage().GetInt(GetID(id), 1) != 0;
        var isSelected = SelectedObject == obj;
        
        var arrowColor = isOpen ?
            Colors.GuiTreeEnabled :
            Colors.GuiTreeDisabled;

        if (obj.Children.Count == 0)
            arrowColor = Colors.Clear;
        
        var flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen;
        if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;
        
        SetCursorPos(new Vector2(GetCursorPosX() - 5f, GetCursorPosY()));
        
        PushStyleColor(ImGuiCol.Text, arrowColor.ToVector4());
        PushStyleColor(ImGuiCol.Header, Colors.GuiTreeSelected.ToVector4());
        var tree = TreeNodeEx(id, flags);
        PopStyleColor();
        PopStyleColor();
        
        // Right click - context
        if (IsItemHovered() && IsMouseReleased(ImGuiMouseButton.Right))
            OpenPopupOnItemClick("context##" + obj.GetHashCode());
        
        // Left click - select
        else if (IsItemHovered() && IsMouseReleased(ImGuiMouseButton.Left)) SelectObject(obj);
        
        // Object context
  
        if (BeginPopup("context##" + obj.GetHashCode())) {
    
            Text(obj.Name);
            
            Separator();

            if (BeginMenu("Insert")) {
                
                //var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract);

                if (MenuItem("Object"))
                    Core.ActiveLevel.RecordedBuildObject("Object", obj);

                Separator();
                
                if (BeginMenu("Lighting")) {
                    
                    if (MenuItem("Directional Light")) {

                        var light = Core.ActiveLevel.RecordedBuildObject("Directional Light", obj);
                        (light.MakeComponent("Light") as Light)?.Type = 0;
                        SelectObject(light);
                    }
                
                    if (MenuItem("Point Light")) {
                            
                        var light = Core.ActiveLevel.RecordedBuildObject("Point Light", obj);
                        (light.MakeComponent("Light") as Light)?.Type = 1;
                        SelectObject(light);
                    }
                
                    if (MenuItem("Spot Light")) {
                            
                        var light = Core.ActiveLevel.RecordedBuildObject("Point Light", obj);
                        (light.MakeComponent("Light") as Light)?.Type = 2;
                        SelectObject(light);
                    }
                    
                    EndMenu();
                }

                if (BeginMenu("Models")) {

                    PathUtil.BestPath("Models", out var checkPath, true);
                    
                    var modelPaths = Directory.GetFiles(checkPath, "*.*", SearchOption.AllDirectories);

                    foreach (var modelPath in modelPaths) {

                        if (Path.GetExtension(modelPath) != ".iqm") continue;

                        var pre = checkPath + "\\";
                        var path = modelPath[pre.Length..^4].Replace('\\', '/');
                        var name = Path.GetFileName(path);

                        if (!MenuItem(path)) continue;

                        var model = Level.MakeObject(name, obj);
                        (model.MakeComponent("Model") as Model)?.Path = path;
                        SelectObject(model);
                    }

                    EndMenu();
                }

                /*
                if (ImGui.BeginMenu("Scripts")) {

                    var modelPaths = Directory.GetFiles(PathUtil.ModRelative("Scripts"), "*.*", SearchOption.AllDirectories);

                    foreach (var modelPath in modelPaths) {

                        if (Path.GetExtension(modelPath) != ".lua") continue;

                        var pre = PathUtil.ModRelative("Scripts") + "\\";
                        var path = modelPath[pre.Length..^4].Replace('\\', '/');
                        var name = Path.GetFileName(path);

                        if (!ImGui.MenuItem(path)) continue;

                        var parentObj = Core.ActiveLevel.RecordedBuildObject(name, obj, null);
                        var script = Core.ActiveLevel.BuildObject("Script", parentObj, "Script").Components["script"] as Script;
                        Core.ActiveLevel.BuildObject("Transform", parentObj, "Transform");
                        script?.Path = path;
                        SelectObject(parentObj);
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("Types")) {

                    foreach (var type in types) {

                        if (!ImGui.MenuItem(type.Name)) continue;

                        var builtObject = Core.ActiveLevel.RecordedBuildObject(type.Name, obj, type.Name);

                        //if (builtObject is { Components: Animation animation, Parent.Components: Model model })
                        //    animation.Path = model.Path;

                        SelectObject(builtObject);
                    }

                    ImGui.EndMenu();
                }
                */

                EndMenu();
            }
            
            if (MenuItem("Delete")) _scheduledDeleteObject = obj;
    
            EndPopup();
        }

        // start drag
        if (BeginDragDropSource()) {
            
            _dragObject = obj;
            
            SetDragDropPayload("object", IntPtr.Zero, 0);
            Text($"Moving {_dragObject.Name}");
            EndDragDropSource();
        }

        // cache drop
        if (BeginDragDropTarget()) {
            
            AcceptDragDropPayload("object");
            
            if (_dragObject != null && IsMouseReleased(ImGuiMouseButton.Left)) {
                
                _dragTarget = obj;
                _savedScroll = GetScrollY();
            }
            
            EndDragDropTarget();
        }
        
        // object icon
        SameLine();
        PushFont(Fonts.ImFontAwesomeSmall);
        SetCursorPos(new(GetCursorPosX() - 10f, GetCursorPosY() + 2.5f));
        TextColored(Colors.GuiTypeObject.ToVector4(), Icons.Obj);
        PopFont();

        // object name
        SameLine();
        PushFont(Fonts.ImMontserratRegular);
        SetCursorPos(new(GetCursorPosX() - 2.0f, GetCursorPosY() - 1.5f));
        TextColored(new(1, 1, 1, 1), obj.Name);
        PopFont();

        // draw child nodes
        if (!tree) return true;
        
        _lastRowY[indent + 1] = centerY;
        
        if (obj.Children.Any(child => !DrawObject(child.Value, indent + 1))) {
            
            TreePop();
            return false;
        }
            
        TreePop();

        return true;
    }

    public static void SelectObject(Obj? obj) {
        
        SelectedObject?.IsSelected = false;
        SelectedObject = obj;
        if (obj != null) obj.IsSelected = true;
    }

    public static void Delete(Obj? obj) => _scheduledDeleteObject = obj;
    public static void DeleteSelectedObject() {
        
        if (!CanDeleteSelectedObject) return;
        _scheduledDeleteObject = SelectedObject;
    }

    public static bool CanDeleteSelectedObject => SelectedObject != Core.ActiveLevel?.Root;
}