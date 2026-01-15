using System.Numerics;
using Raylib_cs;

internal class Camera(Obj obj, Camera3D cam) : ObjType(obj) {
    
    public override string LabelIcon => Icons.Camera;
    public override Color LabelColor => Colors.GuiTypeCamera;

    public Camera3D Cam = cam;
    
    public override bool Load(Core core, bool isEditor) => true;

    public override void Loop3D(Core core, bool isEditor) {

        var pos = Vector3.Zero;
        var rot= Quaternion.Identity;
        
        obj.Parent?.DecomposeMatrix(out pos, out rot, out _);
        
        var forward = Vector3.Transform(Vector3.UnitZ, rot);

        Cam.Position = pos;
        Cam.Target = (pos + forward);

        Cam.DrawCameraFrustum(Raylib.ColorAlpha(Raylib_cs.Color.White, IsSelected ? 1 : 0.3f));
    }

    public override void LoopUi(Core core, bool isEditor) {
        
        
    }

    public override void Loop3DEditor(Core core, Viewport viewport) {
        
        
    }

    public override void LoopUiEditor(Core core, Viewport viewport) {
        
        
    }

    public override void Quit() {
        
        
    }
}