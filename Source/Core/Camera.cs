using System.Numerics;
using Raylib_cs;

internal class Camera(Obj obj) : Component(obj, "camera") {

    public override string LabelIcon => Icons.Camera;
    public override Color LabelScytheColor => Colors.GuiTypeCamera;

    public required Camera3D Cam = new();

    public override void Loop(bool is2D) {

        if (is2D) return;

        Obj.DecomposeMatrix(out var pos, out var rot, out _);
        
        var forward = Vector3.Transform(Vector3.UnitZ, rot);

        Cam.Position = pos;
        Cam.Target = (pos + forward);

        if (!CommandLine.Editor) return;
        
        Cam.DrawCameraFrustum(Raylib.ColorAlpha(Color.White, IsSelected ? 1 : 0.3f));
    }
}