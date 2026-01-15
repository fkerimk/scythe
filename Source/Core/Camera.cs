
using System.Numerics;
using Raylib_cs;

internal class Camera(Obj obj) : ObjType(obj) {
    
    public override string LabelIcon => Icons.Camera;
    public override Color LabelColor => Colors.GuiTypeCamera;

    public Cam? Cam;

    public override bool Load(Core core, bool isEditor) {

        Cam = new Cam();
        return true;
    }

    public override void Loop3D(Core core, bool isEditor) {

        var pos = Vector3.Zero;
        var rot= Quaternion.Identity;
        var scale = Vector3.One;
        
        obj.Parent?.DecomposeMatrix(out pos, out rot, out scale);
        
        var forward = Vector3.Transform(Vector3.UnitZ, rot);

        Cam?.Pos = pos.to_float3();
        Cam?.Target = (pos + forward).to_float3();
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