using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using Jitter2.Collision.Shapes;

internal class SphereCollider(Obj obj) : Component(obj) {

    public override Color LabelColor => Colors.GuiTypePhysics;
    public override string LabelIcon => Icons.FaDotCircleO;

    [RecordHistory] [JsonProperty] [Label("Radius")] public float Radius { get; set; } = 0.5f;
    [RecordHistory] [JsonProperty] [Label("Center")] public Vector3 Center { get; set; } = Vector3.Zero;

    [JsonIgnore] public SphereShape? Shape;

    public override bool Load() {
        
        Obj.DecomposeWorldMatrix(out _, out _, out var scale);
        // Küre için uniform scale varsayıyoruz, en büyüğünü alalım
        float maxScale = MathF.Max(scale.X, MathF.Max(scale.Y, scale.Z));
        Shape = new SphereShape(Radius * maxScale);
        return true;
    }

    public override void Render3D() {
        
        if (!IsSelected || !CommandLine.Editor) return;
        
        Rlgl.PushMatrix();
        Rlgl.MultMatrixf(Obj.WorldMatrix);
        Raylib.DrawSphereWires(Center, Radius, 8, 8, Raylib.ColorAlpha(Color.Green, 0.5f));
        Rlgl.PopMatrix();
    }
}
