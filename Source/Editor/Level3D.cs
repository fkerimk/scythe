using System.Numerics;
using ImGuiNET;
using Raylib_cs;

internal class Level3D() : Viewport("3D") {

    public RenderTexture2D Rt = new();
    public Vector2 TexSize = Vector2.One, TexTemp = Vector2.Zero;
    public Vector2 RelativeMouse3D;

    protected override void OnDraw() {
        
        if (Rt.Texture.Width == 0 || Rt.Texture.Height == 0) {
                
            ImGui.End();
            return;
        }

        var tex = (IntPtr)Rt.Texture.Id;
        
        var contentPos = ImGui.GetCursorScreenPos();
        
        ImGui.Image(tex, ContentRegion, new(0, 1), new(1, 0));
        
        var mouse = Raylib.GetMousePosition();

        var relX = Raymath.Clamp((mouse.X - contentPos.X) / ContentRegion.X, 0, 1);
        var relY = Raymath.Clamp((mouse.Y - contentPos.Y) / ContentRegion.Y, 0, 1);

        relX = (relX - 0.5f) * (ContentRegion.X / Raylib.GetScreenWidth()) * (Raylib.GetScreenHeight() / ContentRegion.Y) + 0.5f;

        RelativeMouse3D = new(relX * Raylib.GetScreenWidth(), relY * Raylib.GetScreenHeight());
        
        TexSize = ContentRegion;
    }
}