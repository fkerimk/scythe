using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class level_3d() : viewport("3D", ImGuiWindowFlags.NoCollapse) {

    public RenderTexture2D rt = new();
    public Vector2 tex_size = Vector2.One, tex_temp = Vector2.Zero;

    protected override void on_draw() {
        
        if (rt.Texture.Width == 0 || rt.Texture.Height == 0) {
                
            ImGui.End();
            return;
        }

        var tex = (IntPtr)rt.Texture.Id;

        ImGui.Image(tex, content_region, new(0, 1), new(1, 0));
        
        tex_size = content_region;
    }
}