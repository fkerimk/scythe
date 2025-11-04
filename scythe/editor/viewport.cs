using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public static class viewport {

    public static RenderTexture2D rt;
    public static Vector2 pos, size, tex_size, tex_temp;

    public static void init() {
        
        rt = new();
        tex_size = Vector2.One;
        tex_temp = Vector2.Zero;
    }
    
    public static void draw() {

        if (!ImGui.Begin("Viewport", ImGuiWindowFlags.NoCollapse)) { ImGui.End(); return; } 
        
        pos = ImGui.GetWindowPos();
        size = ImGui.GetContentRegionAvail();
        
        if (rt.Texture.Width == 0 || rt.Texture.Height == 0) {
                
            ImGui.End();
            return;
        }

        var ratio = (float)screen.width / screen.height;
        var target = new Vector2(size.X, size.Y);

        if (target.X / ratio > target.Y)
            target.X = target.Y * ratio;
        else target.Y = target.X / ratio;

        var offset = new Vector2(
            (size.X - target.X) * 0.5f,
            (size.Y - target.Y) * 0.5f
        );
        
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset.X);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offset.Y);
            
        var tex = (IntPtr)rt.Texture.Id;

        ImGui.Image(tex, new(target.X, target.Y), new(0, 1), new(1, 0));
            
        ImGui.End();
        
        tex_size = target;
    }
}