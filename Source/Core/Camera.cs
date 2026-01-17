using System.Numerics;
using Raylib_cs;

internal class Camera(Obj obj) : ObjType(obj) {

    public Camera() : this(new Obj()) { }
    
    public override string LabelIcon => Icons.Camera;
    public override ScytheColor LabelScytheColor => Colors.GuiTypeCamera;

    public required Camera3D Cam = new();

    public override void Loop3D() {

        var pos = Vector3.Zero;
        var rot= Quaternion.Identity;
        
        Obj.Parent?.DecomposeMatrix(out pos, out rot, out _);
        
        var forward = Vector3.Transform(Vector3.UnitZ, rot);

        Cam.Position = pos;
        Cam.Target = (pos + forward);

        if (!CommandLine.Editor) return;
        
        Cam.DrawCameraFrustum(Raylib.ColorAlpha(Color.White, IsSelected ? 1 : 0.3f));
    }
}