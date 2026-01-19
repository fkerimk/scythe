using System.Numerics;
using ImGuiNET;

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

        ImGui.BeginChild("scroll", new Vector2(0, 0));
        
        // restore scroll 
        if (_savedScroll != null) {
            
            ImGui.SetScrollY(_savedScroll.Value);
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
        
        ImGui.EndChild();
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

        var drawList = ImGui.GetWindowDrawList();
        var rowPos = ImGui.GetCursorScreenPos();
        var rowHeight = ImGui.GetFrameHeight();
        var centerY = rowPos.Y + rowHeight / 2f;
        
        // Zebra stripping
        if (_rowCount % 2 == 0) {
            
            drawList.AddRectFilled(
                
                new Vector2(ImGui.GetWindowPos().X, rowPos.Y),
                new Vector2(ImGui.GetWindowPos().X + ImGui.GetWindowWidth(), rowPos.Y + rowHeight),
                ImGui.GetColorU32(new Vector4(1, 1, 1, 0.012f))
            );
        }
        _rowCount++;

        // Hierarchy lines
        var lineCol = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.15f));
        
        if (indent > 0) {
            
            var indentStep = ImGui.GetStyle().IndentSpacing; 
            var lineX = rowPos.X - indentStep + 10f;
            
            // Horizontal line
            drawList.AddLine(new Vector2(lineX, centerY), new Vector2(lineX + 11f, centerY), lineCol);
            
            // Vertical line
            if (_lastRowY[indent] > 0)
                drawList.AddLine(new Vector2(lineX, _lastRowY[indent]), new Vector2(lineX, centerY), lineCol);
        }
        _lastRowY[indent] = centerY;

        if (SelectedObject != null && IsAncestorOf(obj, SelectedObject)) {
            
            ImGui.SetNextItemOpen(true);
        }
        
        // tree node
        var id = "##" + obj.GetHashCode();
        var isOpen = ImGui.GetStateStorage().GetInt(ImGui.GetID(id), 1) != 0;
        var isSelected = SelectedObject == obj;
        
        var arrowColor = isOpen ?
            Colors.GuiTreeEnabled.to_vector4() :
            Colors.GuiTreeDisabled.to_vector4();

        if (obj.Children.Count == 0)
            arrowColor = Colors.Clear.to_vector4();
        
        var flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen;
        if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;
        
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 5f, ImGui.GetCursorPosY()));
        
        ImGui.PushStyleColor(ImGuiCol.Text, arrowColor);
        ImGui.PushStyleColor(ImGuiCol.Header, Colors.GuiTreeSelected.to_vector4());
        var tree = ImGui.TreeNodeEx(id, flags);
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        
        // Right click - context
        if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            ImGui.OpenPopupOnItemClick("context##" + obj.GetHashCode());
        
        // Left click - select
        else if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) SelectObject(obj);
        
        // Object context
  
        if (ImGui.BeginPopup("context##" + obj.GetHashCode())) {
    
            ImGui.Text(obj.Name);
            
            ImGui.Separator();

            if (ImGui.BeginMenu("Insert")) {
                
                //var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract);

                if (ImGui.MenuItem("Object"))
                    Core.ActiveLevel.RecordedBuildObject("Object", obj);

                ImGui.Separator();
                
                if (ImGui.BeginMenu("Lighting")) {
                    
                    if (ImGui.MenuItem("Directional Light")) {

                        var light = Core.ActiveLevel.RecordedBuildObject("Directional Light", obj);
                        (light.MakeComponent("Light") as Light)?.Type = 0;
                        SelectObject(light);
                    }
                
                    if (ImGui.MenuItem("Point Light")) {
                            
                        var light = Core.ActiveLevel.RecordedBuildObject("Point Light", obj);
                        (light.MakeComponent("Light") as Light)?.Type = 1;
                        SelectObject(light);
                    }
                
                    if (ImGui.MenuItem("Spot Light")) {
                            
                        var light = Core.ActiveLevel.RecordedBuildObject("Point Light", obj);
                        (light.MakeComponent("Light") as Light)?.Type = 2;
                        SelectObject(light);
                    }
                    
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Models")) {

                    PathUtil.BestPath("Models", out var checkPath, true);
                    
                    var modelPaths = Directory.GetFiles(checkPath, "*.*", SearchOption.AllDirectories);

                    foreach (var modelPath in modelPaths) {

                        if (Path.GetExtension(modelPath) != ".iqm") continue;

                        var pre = checkPath + "\\";
                        var path = modelPath[pre.Length..^4].Replace('\\', '/');
                        var name = Path.GetFileName(path);

                        if (!ImGui.MenuItem(path)) continue;

                        var model = Level.MakeObject(name, obj);
                        (model.MakeComponent("Model") as Model)?.Path = path;
                        SelectObject(model);
                    }

                    ImGui.EndMenu();
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

                ImGui.EndMenu();
            }
            
            if (ImGui.MenuItem("Delete")) _scheduledDeleteObject = obj;
    
            ImGui.EndPopup();
        }

        // start drag
        if (ImGui.BeginDragDropSource()) {
            
            _dragObject = obj;
            
            ImGui.SetDragDropPayload("object", IntPtr.Zero, 0);
            ImGui.Text($"Moving {_dragObject.Name}");
            ImGui.EndDragDropSource();
        }

        // cache drop
        if (ImGui.BeginDragDropTarget()) {
            
            ImGui.AcceptDragDropPayload("object");
            
            if (_dragObject != null && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                
                _dragTarget = obj;
                _savedScroll = ImGui.GetScrollY();
            }
            
            ImGui.EndDragDropTarget();
        }
        
        // object icon
        ImGui.SameLine();
        ImGui.PushFont(Fonts.ImFontAwesomeSmall);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 10f, ImGui.GetCursorPosY() + 2.5f));
        ImGui.TextColored(Colors.GuiTypeObject.to_vector4(), Icons.Obj);
        ImGui.PopFont();

        // object name
        ImGui.SameLine();
        ImGui.PushFont(Fonts.ImMontserratRegular);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 2.0f, ImGui.GetCursorPosY() - 1.5f));
        ImGui.TextColored(new(1, 1, 1, 1), obj.Name);
        ImGui.PopFont();

        // draw child nodes
        if (!tree) return true;
        
        _lastRowY[indent + 1] = centerY;
        
        if (obj.Children.Any(child => !DrawObject(child.Value, indent + 1))) {
            
            ImGui.TreePop();
            return false;
        }
            
        ImGui.TreePop();

        return true;
    }

    private static void SelectObject(Obj? obj) {
        
        SelectedObject?.IsSelected = false;
        SelectedObject = obj;
        obj?.IsSelected = true;
    }

    public static void Delete(Obj? obj) => _scheduledDeleteObject = obj;
    public static void DeleteSelectedObject() {
        
        if (!CanDeleteSelectedObject) return;
        _scheduledDeleteObject = SelectedObject;
    }

    public static bool CanDeleteSelectedObject => SelectedObject != Core.ActiveLevel?.Root;
}