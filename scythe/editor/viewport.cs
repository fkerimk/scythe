using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public abstract class viewport(string title, ImGuiWindowFlags flags) {

    public custom_style? custom_style;
    
    public Vector2 window_pos;
    public Vector2 content_region;
    public Vector2 relative_mouse;
    public Vector2 relative_mouse_3d;
    
    public bool isHovered;
    
    public void draw() {

        if (custom_style != null) style.push(custom_style);
        
        if (!ImGui.Begin(title, flags)) {

            isHovered = false;
            
            ImGui.End(); 
            
            return;
        }
        
        window_pos = ImGui.GetWindowPos();
        content_region = ImGui.GetContentRegionAvail();
        relative_mouse = ImGui.GetMousePos() - window_pos - ImGui.GetCursorStartPos();
        
        relative_mouse_3d = Raylib.GetScreenCenter()
            + Raylib.GetMousePosition()
            - ImGui.GetWindowPos()
            - new Vector2(-ImGui.GetStyle().FramePadding.X, ImGui.GetStyle().FramePadding.Y * 2)
            - ImGui.GetWindowSize() * 0.5f;
        
        isHovered = ImGui.IsWindowHovered();
        
        on_draw();
        
        ImGui.End();
        
        if (custom_style != null) style.pop();
    }

    protected abstract void on_draw();
}