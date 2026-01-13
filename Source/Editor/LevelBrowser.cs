using System.Numerics;
using ImGuiNET;

internal class LevelBrowser() : Viewport("Level") {

    // cache
    public Obj? DragObject;
    public Obj? DragTarget;
    public Obj? DeleteObject;
    public Obj? SelectedObject;
    public float? SavedScroll;

    protected override void OnDraw() {

        if (Core.ActiveLevel == null) return;
        
        ImGui.BeginChild("scroll", new Vector2(0, 0));
        
        // restore scroll 
        if (SavedScroll != null) {
            
            ImGui.SetScrollY(SavedScroll.Value);
            SavedScroll = null;
        }

        // drag object
        if (DragObject != null && DragTarget != null) {

            DragObject.set_parent(DragTarget);
            
            DragObject = null;
            DragTarget = null;
        }

        // delete object
        if (DeleteObject != null) {
            
            DeleteObject.Delete();
            DeleteObject = null;
        }
        
        // draw objects
        draw_object(Core.ActiveLevel.Root);
        
        ImGui.EndChild();
    }

    private bool draw_object(Obj obj, int indent = 0) {

        if (Core.ActiveLevel == null) return true;
        
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
        
        // right click - context
        if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            ImGui.OpenPopupOnItemClick("context##" + obj.GetHashCode());
        
        // left click - select
        else if (ImGui.IsItemHovered() && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) select_object(obj);
        
        // object context
        if (ImGui.BeginPopup("context##" + obj.GetHashCode())) {
    
            ImGui.Text(obj.Name);
            
            ImGui.Separator();
            
            if (ImGui.MenuItem("Insert")) Level.BuildObject("object", obj);
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
        ImGui.PushFont(Fonts.FontAwesomeSmall);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 15, ImGui.GetCursorPosY() + 2.5f));
        ImGui.TextColored(obj.Color.to_vector4(), obj.Icon);
        ImGui.PopFont();

        // object name
        ImGui.SameLine();
        ImGui.PushFont(Fonts.MontserratRegular);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 2.5f, ImGui.GetCursorPosY() - 1.5f));
        ImGui.TextColored(new(1, 1, 1, 1), obj.Name);
        ImGui.PopFont();

        // draw child nodes
        if (!tree) return true;
        
        if (obj.Children.Any(child => !draw_object(child, indent + 1))) {
            
            ImGui.TreePop();
            return false;
        }
            
        ImGui.TreePop();

        return true;
    }

    public void select_object(Obj? obj) {
        
        SelectedObject?.IsSelected = false;
        SelectedObject = obj;
        obj?.IsSelected = true;
    }
}