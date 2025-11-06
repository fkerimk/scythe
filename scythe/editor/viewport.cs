using System.Numerics;
using ImGuiNET;

namespace scythe;

public abstract class viewport(string title, ImGuiWindowFlags flags) {

    public bool isHovered;
    public Vector2 pos, size;
    
    public void draw() {

        if (!ImGui.Begin(title, flags)) {

            isHovered = false;
            
            ImGui.End(); 
            
            return;
        }
        
        pos = ImGui.GetWindowPos();
        size = ImGui.GetContentRegionAvail();
        isHovered = ImGui.IsWindowHovered();
        
        on_draw();
        
        ImGui.End();
    }
    
    public abstract void on_draw();
}