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
        
        //relative_mouse_3d = Raylib.GetScreenCenter()
        //    + ImGui.GetMousePos()
        //    - ImGui.GetWindowPos()
        //    //- new Vector2(-ImGui.GetStyle().FramePadding.X, ImGui.GetStyle().FramePadding.Y * 2)
        //    - content_region * 0.5f;
        
        IsHovered = ImGui.IsWindowHovered();
        
        OnDraw();
        
        ImGui.End();
        
        if (CustomStyle != null) Style.Pop();
    }

    protected abstract void OnDraw();
}