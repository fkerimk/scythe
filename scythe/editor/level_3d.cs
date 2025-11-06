using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class level_3d() : viewport("3D", ImGuiWindowFlags.NoCollapse) {

    public RenderTexture2D rt = new();
    public Vector2 tex_size = Vector2.One, tex_temp = Vector2.Zero;

    public override void on_draw() {
        
        if (rt.Texture.Width == 0 || rt.Texture.Height == 0) {
                
            ImGui.End();
            return;
        }

        //var ratio = (float)screen.width / screen.height;
        //var target = new Vector2(size.X, size.Y);
        //
        //if (target.X / ratio > target.Y)
        //    target.X = target.Y * ratio;
        //else target.Y = target.X / ratio;
        //
        //var offset = new Vector2(
        //    (size.X - target.X) * 0.5f,
        //    (size.Y - target.Y) * 0.5f
        //);
        //
        //ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset.X);
        //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offset.Y);
            
        var tex = (IntPtr)rt.Texture.Id;

        ImGui.Image(tex, size, new(0, 1), new(1, 0));
        
        tex_size = size;
    }
}