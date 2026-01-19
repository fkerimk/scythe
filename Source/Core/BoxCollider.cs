using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using Jitter2.Collision.Shapes;

internal class BoxCollider(Obj obj) : Component(obj) {

    public override Color LabelColor => Colors.GuiTypeObject;
    public override string LabelIcon => Icons.Box;

    [RecordHistory] [JsonProperty] [Label("Size")] public Vector3 Size { get; set; } = Vector3.One;
    [RecordHistory] [JsonProperty] [Label("Center")] public Vector3 Center { get; set; } = Vector3.Zero;

    [JsonIgnore] public BoxShape? Shape;

    public override bool Load() {
        
        Obj.DecomposeWorldMatrix(out _, out _, out var scale);
        
        Shape = new BoxShape(Size.X * scale.X, Size.Y * scale.Y, Size.Z * scale.Z);
        return true;
    }

    public override void Loop(bool is2D) {
        
        if (is2D) return;

        if (!IsSelected || !CommandLine.Editor) return;
        
        var gizmoMatrix = Matrix4x4.CreateScale(Size) * Matrix4x4.CreateTranslation(Center) * Obj.WorldMatrix;
            
        Rlgl.PushMatrix();
        Rlgl.MultMatrixf(gizmoMatrix);
        Raylib.DrawCubeWires(Vector3.Zero, 1.0f, 1.0f, 1.0f, Raylib.ColorAlpha(Color.Green, 0.5f));
        Rlgl.PopMatrix();
    }
}
