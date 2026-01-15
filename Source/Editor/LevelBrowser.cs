using System.Numerics;
using System.Reflection;
using ImGuiNET;

internal class LevelBrowser(Editor editor) : Viewport("Level") {

    // cache
    public Obj? DragObject;
    public Obj? DragTarget;
    public Obj? DeleteObject;
    public Obj? SelectedObject;
    public float? SavedScroll;

    protected override void OnDraw() {

        if (editor.Core.ActiveLevel == null) return;
        
        ImGui.BeginChild("scroll", new Vector2(0, 0));
        
        // restore scroll 
        if (SavedScroll != null) {
            
            ImGui.SetScrollY(SavedScroll.Value);
            SavedScroll = null;
        }

        // drag object
        if (DragObject != null && DragTarget != null) {

            DragObject.SetParent(DragTarget);
            
            DragObject = null;
            DragTarget = null;
        }

        // Delete object
        if (DeleteObject != null) {

            if (DeleteObject != editor.Core.ActiveLevel.Root) {
                
                DeleteObject.RecordedDelete();
            }
            
            DeleteObject = null;
        }
        
        // draw objects
        DrawObject(editor.Core.ActiveLevel.Root);
        
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

        if (editor.Core.ActiveLevel == null) return true;

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

                if (ImGui.MenuItem("Object")) editor.Core.ActiveLevel.RecordedBuildObject("Object", obj, null);

                ImGui.Separator();
                
                if (ImGui.BeginMenu("Lighting")) {
                    
                    if (ImGui.MenuItem("Directional Light")) {
                            
                        var lightParent = editor.Core.ActiveLevel.RecordedBuildObject("Directional Light", obj, null);
                        var light = editor.Core.ActiveLevel.BuildObject("Light", lightParent, "Light").Type as Light;
                        editor.Core.ActiveLevel.BuildObject("Transform", lightParent, "Transform");
                        light?.Type = 0;
                        SelectObject(lightParent);
                    }
                
                    if (ImGui.MenuItem("Point Light")) {
                            
                        var lightParent = editor.Core.ActiveLevel.RecordedBuildObject("Point Light", obj, null);
                        var light = editor.Core.ActiveLevel.BuildObject("Light", lightParent, "Light").Type as Light;
                        editor.Core.ActiveLevel.BuildObject("Transform", lightParent, "Transform");
                        light?.Type = 1;
                        SelectObject(lightParent);
                    }
                
                    if (ImGui.MenuItem("Spot Light")) {
                            
                        var lightParent = editor.Core.ActiveLevel.RecordedBuildObject("Spot Light", obj, null);
                        var light = editor.Core.ActiveLevel.BuildObject("Light", lightParent, "Light").Type as Light;
                        editor.Core.ActiveLevel.BuildObject("Transform", lightParent, "Transform");
                        light?.Type = 2;
                        SelectObject(lightParent);
                    }
                    
                    ImGui.EndMenu();
                }
                
                if (ImGui.BeginMenu("Models")) {

                    var modelPaths = Directory.GetFiles(PathUtil.Relative("Models"), "*.*", SearchOption.AllDirectories);

                    foreach (var modelPath in modelPaths) {

                        if (Path.GetExtension(modelPath) != ".iqm") continue;
                        
                        var path = modelPath.TrimStart(PathUtil.Relative("Models")+"\\")[..^4].ToString().Replace('\\', '/');
                        
                        if (!ImGui.MenuItem(path)) continue;
                        
                        var parentObj = editor.Core.ActiveLevel.RecordedBuildObject(path, obj, null);
                        var model = editor.Core.ActiveLevel.BuildObject("Model", parentObj, "Model").Type as Model;
                        editor.Core.ActiveLevel.BuildObject("Transform", parentObj, "Transform");
                        model?.Path = path;
                        SelectObject(parentObj);
                    }
                    
                    ImGui.EndMenu();
                }
                
                ImGui.Separator();
                
                if (ImGui.BeginMenu("Types")) {
                    
                    foreach (var type in types) {
                        
                        if (!ImGui.MenuItem(type.Name)) continue;
                        
                        var builtObject = editor.Core.ActiveLevel.RecordedBuildObject(type.Name, obj, type.Name);

                        if (builtObject.Type is Animation animation) {

                            if (builtObject.Parent?.Type is Model model) {

                                animation.Path = model.Path;
                            }
                        }
                        
                        SelectObject(builtObject);
                    }
                    
                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }
            
            if (ImGui.MenuItem("Rename")) { }
            if (ImGui.MenuItem("Delete")) DeleteObject = obj;
    
            ImGui.EndPopup();
        }

        // start drag
        if (ImGui.BeginDragDropSource()) {
            
            DragObject = obj;
            
            ImGui.SetDragDropPayload("object", IntPtr.Zero, 0);
            ImGui.Text($"Moving {DragObject.Name}");
            ImGui.EndDragDropSource();
        }

        // cache drop
        if (ImGui.BeginDragDropTarget()) {
            
            ImGui.AcceptDragDropPayload("object");
            
            if (DragObject != null && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                
                DragTarget = obj;
                SavedScroll = ImGui.GetScrollY();
            }
            
            ImGui.EndDragDropTarget();
        }
        
        // object icon
        ImGui.SameLine();
        ImGui.PushFont(Fonts.ImFontAwesomeSmall);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 15, ImGui.GetCursorPosY() + 2.5f));
        ImGui.TextColored(obj.Color.to_vector4(), obj.Icon);
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