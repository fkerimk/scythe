using System.Numerics;
using System.Reflection;
using ImGuiNET;

internal class LevelBrowser() : Viewport("Level") {

    // cache
    private float? _savedScroll;

    private Obj?
        _dragObject,
        _dragTarget;
    
    public Obj? DeleteObject;

    public Obj? SelectedObject { get; private set; }
    
    protected override void OnDraw() {

        if (Core.ActiveLevel == null) return;
        
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
        if (DeleteObject != null) {

            if (DeleteObject != Core.ActiveLevel.Root) {
                
                DeleteObject.RecordedDelete();
            }
            
            DeleteObject = null;
        }
        
        // draw objects
        DrawObject(Core.ActiveLevel.Root);
        
        ImGui.EndChild();
    }

    private bool IsAncestorOf(Obj ancestor, Obj target) {

        var current = target.Parent;

        while (current != null) {
            
            if (current == ancestor) return true;
            current = current.Parent;
        }
        
        return false;
    }

    private bool DrawObject(Obj obj, int indent = 0) {

        if (Core.ActiveLevel == null) return true;

        if (SelectedObject != null && IsAncestorOf(obj, SelectedObject)) {
            
            ImGui.SetNextItemOpen(true);
        }
        
        // tree node
        var isOpen = ImGui.GetStateStorage().GetInt(ImGui.GetID(" ##" + obj.GetHashCode()), 1) != 0;
        var isSelected = SelectedObject == obj;
        
        var arrowColor = isOpen ?
            Colors.GuiTreeEnabled.to_vector4() :
            Colors.GuiTreeDisabled.to_vector4();

        if (obj.Children.Count == 0)
            arrowColor = Colors.Clear.to_vector4();
        
        var flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;
        if (isSelected) flags |= ImGuiTreeNodeFlags.Selected;
        
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - indent * 7.5f - 7.5f, ImGui.GetCursorPosY()));
        
        ImGui.PushStyleColor(ImGuiCol.Text, arrowColor);
        ImGui.PushStyleColor(ImGuiCol.Header, Colors.GuiTreeSelected.to_vector4());
        var tree = ImGui.TreeNodeEx(" ##" + obj.GetHashCode(), flags);
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
                
                var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(ObjType)) && !t.IsAbstract);

                if (ImGui.MenuItem("Object")) Core.ActiveLevel.RecordedBuildObject("Object", obj, null);

                ImGui.Separator();
                
                if (ImGui.BeginMenu("Lighting")) {
                    
                    if (ImGui.MenuItem("Directional Light")) {
                            
                        var lightParent = Core.ActiveLevel.RecordedBuildObject("Directional Light", obj, null);
                        var light = Core.ActiveLevel.BuildObject("Light", lightParent, "Light").Type as Light;
                        Core.ActiveLevel.BuildObject("Transform", lightParent, "Transform");
                        light?.Type = 0;
                        SelectObject(lightParent);
                    }
                
                    if (ImGui.MenuItem("Point Light")) {
                            
                        var lightParent = Core.ActiveLevel.RecordedBuildObject("Point Light", obj, null);
                        var light = Core.ActiveLevel.BuildObject("Light", lightParent, "Light").Type as Light;
                        Core.ActiveLevel.BuildObject("Transform", lightParent, "Transform");
                        light?.Type = 1;
                        SelectObject(lightParent);
                    }
                
                    if (ImGui.MenuItem("Spot Light")) {
                            
                        var lightParent = Core.ActiveLevel.RecordedBuildObject("Spot Light", obj, null);
                        var light = Core.ActiveLevel.BuildObject("Light", lightParent, "Light").Type as Light;
                        Core.ActiveLevel.BuildObject("Transform", lightParent, "Transform");
                        light?.Type = 2;
                        SelectObject(lightParent);
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Models")) {

                    var modelPaths = Directory.GetFiles(PathUtil.ModRelative("Models"), "*.*", SearchOption.AllDirectories);

                    foreach (var modelPath in modelPaths) {

                        if (Path.GetExtension(modelPath) != ".iqm") continue;
                        
                        var pre = PathUtil.ModRelative("Models") + "\\";
                        var path = modelPath[pre.Length..^4].Replace('\\', '/');
                        var name = Path.GetFileName(path);
                        
                        if (!ImGui.MenuItem(path)) continue;
                        
                        var parentObj = Core.ActiveLevel.RecordedBuildObject(name, obj, null);
                        var model = Core.ActiveLevel.BuildObject("Model", parentObj, "Model").Type as Model;
                        Core.ActiveLevel.BuildObject("Transform", parentObj, "Transform");
                        model?.Path = path;
                        SelectObject(parentObj);
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Scripts")) {

                    var modelPaths = Directory.GetFiles(PathUtil.ModRelative("Scripts"), "*.*", SearchOption.AllDirectories);

                    foreach (var modelPath in modelPaths) {

                        if (Path.GetExtension(modelPath) != ".lua") continue;
                        
                        var pre = PathUtil.ModRelative("Scripts") + "\\";
                        var path = modelPath[pre.Length..^4].Replace('\\', '/');
                        var name = Path.GetFileName(path);
                        
                        if (!ImGui.MenuItem(path)) continue;
                        
                        var parentObj = Core.ActiveLevel.RecordedBuildObject(name, obj, null);
                        var script = Core.ActiveLevel.BuildObject("Script", parentObj, "Script").Type as Script;
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

                        if (builtObject is { Type: Animation animation, Parent.Type: Model model })
                            animation.Path = model.Path;
                        
                        SelectObject(builtObject);
                    }
                    
                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }
            
            if (ImGui.MenuItem("Delete")) DeleteObject = obj;
    
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
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 15, ImGui.GetCursorPosY() + 2.5f));
        ImGui.TextColored(obj.ScytheColor.to_vector4(), obj.Icon);
        ImGui.PopFont();

        // object name
        ImGui.SameLine();
        ImGui.PushFont(Fonts.ImMontserratRegular);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 2.5f, ImGui.GetCursorPosY() - 1.5f));
        ImGui.TextColored(new(1, 1, 1, 1), obj.Name);
        ImGui.PopFont();

        // draw child nodes
        if (!tree) return true;
        
        if (obj.Children.Any(child => !DrawObject(child, indent + 1))) {
            
            ImGui.TreePop();
            return false;
        }
            
        ImGui.TreePop();

        return true;
    }

    public void SelectObject(Obj? obj) {
        
        SelectedObject?.IsSelected = false;
        SelectedObject = obj;
        obj?.IsSelected = true;
    }
}