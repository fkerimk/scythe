using System.Numerics;
using ImGuiNET;

internal abstract class Viewport(string title) {

    public CustomStyle? CustomStyle;
    
    public Vector2 WindowPos;
    public Vector2 ContentRegion;
    public Vector2 RelativeMouse;
    
    public bool IsHovered;
    
    public void Draw() {

        if (CustomStyle != null) Style.Push(CustomStyle);
        
        if (!ImGui.Begin(title, ImGuiWindowFlags.NoCollapse)) {

            IsHovered = false;
            
            ImGui.End(); 
            
            if (CustomStyle != null) Style.Pop();
            return;
        }
        
        WindowPos = ImGui.GetWindowPos();
        ContentRegion = ImGui.GetContentRegionAvail();
        RelativeMouse = ImGui.GetMousePos() - WindowPos - ImGui.GetCursorStartPos();
        IsHovered = ImGui.IsWindowHovered();
        
        OnDraw();
        
        ImGui.End();
        
        if (CustomStyle != null) Style.Pop();
    }

    protected abstract void OnDraw();
}