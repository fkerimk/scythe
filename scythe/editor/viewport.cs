using System.Numerics;
using ImGuiNET;

namespace scythe;

public abstract class viewport(string title, ImGuiWindowFlags flags) {

    public Vector2 pos, size, mouse, cursor, cursor_start, relative_mouse;
    public float title_height;
    public bool isHovered;
    
    public void draw() {

        if (!ImGui.Begin(title, flags)) {

            isHovered = false;
            
            ImGui.End(); 
            
            return;
        }

        
        pos = ImGui.GetWindowPos();
        size = ImGui.GetContentRegionAvail();
        mouse = ImGui.GetMousePos();
        cursor = ImGui.GetCursorPos();
        cursor_start = ImGui.GetCursorStartPos();
        title_height = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;
        relative_mouse = mouse - pos - cursor_start;

        isHovered = ImGui.IsWindowHovered();
        
        on_draw();
        
        ImGui.End();
    }
    
    public abstract void on_draw();
}