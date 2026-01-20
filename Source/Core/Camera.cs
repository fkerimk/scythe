using System.Numerics;
using Raylib_cs;

internal class Camera(Obj obj) : Component(obj) {

    public override string LabelIcon => Icons.FaVideoCamera;
    public override Color LabelColor => Colors.GuiTypeCamera;

    public required Camera3D Cam = new();

    public override void Logic() {
        
        Obj.DecomposeMatrix(out var pos, out var rot, out _);
        
        var forward = Vector3.Transform(Vector3.UnitZ, rot);

        Cam.Position = pos;
        Cam.Target = (pos + forward);
    }

    public override void Render3D() {

        if (!CommandLine.Editor) return;
        
        Cam.DrawCameraFrustum(Raylib.ColorAlpha(Color.White, IsSelected ? 1 : 0.3f));
    }
}