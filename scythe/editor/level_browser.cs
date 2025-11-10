using ImGuiNET;

namespace scythe;

#pragma warning disable CS8981
public class level_browser(level level) : viewport("Level", ImGuiWindowFlags.NoCollapse) {

    public level level = level;
    
    // cache
    public obj? drag_object;
    public obj? drag_target;
    public obj? delete_object;
    public obj? selected_object;
    public float? saved_scroll;
    
    public override void on_draw() {

        ImGui.BeginChild("scroll", new(0, 0));
        
        // restore scroll 
        if (saved_scroll != null) {
            
            ImGui.SetScrollY(saved_scroll.Value);
            saved_scroll = null;
        }

        // drag object
        if (drag_object != null && drag_target != null) {

            drag_object.set_parent(drag_target);
            
            drag_object = null;
            drag_target = null;
        }

        // delete object
        if (delete_object != null) {
            
            delete_object.delete();
            delete_object = null;
        }
        
        // draw objects
        draw_object(level.root);
        
        ImGui.EndChild();
    }

    private bool draw_object(obj obj, int indent = 0) {

        // tree node
        var is_open = ImGui.GetStateStorage().GetInt(ImGui.GetID(" ##" + obj.GetHashCode()), 1) != 0;
        var is_selected = selected_object == obj;
        
        var arrow_color = is_open ?
            colors.gui_tree_enabled.to_vector4() :
            colors.gui_tree_disabled.to_vector4();

        if (obj.children.Count == 0)
            arrow_color = colors.clear.to_vector4();
        
        var flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanFullWidth;
        if (is_selected) flags |= ImGuiTreeNodeFlags.Selected;
        
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - indent * 12.5f - 7.5f, ImGui.GetCursorPosY()));
        
        ImGui.PushStyleColor(ImGuiCol.Text, arrow_color);
        ImGui.PushStyleColor(ImGuiCol.Header, colors.gui_tree_selected.to_vector4());
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
    
            ImGui.Text(obj.name);
            
            ImGui.Separator();
            
            if (ImGui.MenuItem("Insert")) { }
            if (ImGui.MenuItem("Rename")) { }
            if (ImGui.MenuItem("Delete")) delete_object = obj;
    
            ImGui.EndPopup();
        }

        // start drag
        if (ImGui.BeginDragDropSource()) {
            
            drag_object = obj;
            
            ImGui.SetDragDropPayload("object", IntPtr.Zero, 0);
            ImGui.Text($"Moving {drag_object.name}");
            ImGui.EndDragDropSource();
        }

        // cache drop
        if (ImGui.BeginDragDropTarget()) {
            
            ImGui.AcceptDragDropPayload("object");
            
            if (drag_object != null && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                
                drag_target = obj;
                saved_scroll = ImGui.GetScrollY();
            }
            
            ImGui.EndDragDropTarget();
        }
        
        // object icon
        ImGui.SameLine();
        ImGui.PushFont(fonts.font_awesome_small);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 15, ImGui.GetCursorPosY() + 2.5f));
        
        switch (obj.type) {
            
            case "model"    : ImGui.TextColored(colors.gui_type_model.to_vector4()    , icons.model    ); break;
            case "transform": ImGui.TextColored(colors.gui_type_transform.to_vector4(), icons.transform); break;
            case "animation": ImGui.TextColored(colors.gui_type_animation.to_vector4(), icons.animation); break;
            
            default: ImGui.TextColored(colors.gui_type_object.to_vector4(), icons.obj); break;
        };
        
        ImGui.PopFont();

        // object name
        ImGui.SameLine();
        ImGui.PushFont(fonts.montserrat_regular);
        ImGui.SetCursorPos(new(ImGui.GetCursorPosX() - 2.5f, ImGui.GetCursorPosY() - 1.5f));
        ImGui.TextColored(new(1, 1, 1, 1), obj.name);
        ImGui.PopFont();

        // draw child nodes
        if (!tree) return true;
        
        if (obj.children.Any(child => !draw_object(child, indent + 1))) {
            
            ImGui.TreePop();
            return false;
        }
            
        ImGui.TreePop();

        return true;
    }

    public void select_object(obj? obj) {
        
        selected_object?.is_selected = false;
        selected_object = obj;
        obj?.is_selected = true;
    }
}