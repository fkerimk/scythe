using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public abstract class viewport(string title, ImGuiWindowFlags flags) {

    public Vector2 mouse_pos;
    public Vector2 window_pos;
    public Vector2 window_size;
    public Vector2 content_region;
    public Vector2 cursor_start;
    public float title_height;
    public Vector2 relative_mouse;
    public Vector2 relative_mouse_3d;
    
    public bool isHovered;
    
    public void draw() {

        if (!ImGui.Begin(title, flags)) {

            isHovered = false;
            
            ImGui.End(); 
            
            return;
        }
        
        mouse_pos = ImGui.GetMousePos();
        window_pos = ImGui.GetWindowPos();
        window_size = ImGui.GetWindowSize();
        content_region = ImGui.GetContentRegionAvail();
        cursor_start = ImGui.GetCursorStartPos();
        title_height = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;
        relative_mouse = mouse_pos - window_pos - cursor_start;
        var center = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
        relative_mouse_3d = center + Raylib.GetMousePosition() - window_pos - ImGui.GetWindowSize() * 0.5f;
        
        //var window_relative_mouse = Raylib.GetWindowPosition() + Raylib.GetMousePosition() - Raylib.GetWindowPosition() ;
        //relative_mouse_3d = window_relative_mouse;
        
        isHovered = ImGui.IsWindowHovered();
        
        on_draw();
        
        ImGui.End();
    }
    
    public abstract void on_draw();
}