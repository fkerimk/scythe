using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using Jitter2.Collision.Shapes;

internal class BoxCollider(Obj obj) : Component(obj) {

    public override Color LabelColor => Colors.GuiTypePhysics;
    public override string LabelIcon => Icons.FaCube;

    [RecordHistory] [JsonProperty] [Label("Size")] public Vector3 Size { get; set; } = Vector3.One;
    [RecordHistory] [JsonProperty] [Label("Center")] public Vector3 Center { get; set; } = Vector3.Zero;

    [JsonIgnore] public BoxShape? Shape;

    public override bool Load() {
        
        Obj.DecomposeWorldMatrix(out _, out _, out var scale);
        Shape = new BoxShape(Size.X * scale.X, Size.Y * scale.Y, Size.Z * scale.Z);
        return true;
    }

    public override void Render3D() {
        
        if (!IsSelected || !CommandLine.Editor) return;
        
        Rlgl.PushMatrix();
        Rlgl.MultMatrixf(Obj.WorldMatrix);
        Raylib.DrawCubeWires(Center, Size.X, Size.Y, Size.Z, Raylib.ColorAlpha(Color.Green, 0.5f));
        Rlgl.PopMatrix();
    }
}
