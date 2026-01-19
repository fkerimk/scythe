using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using Jitter2.Collision.Shapes;

internal class BoxCollider(Obj obj) : Component(obj) {

    public override Color LabelColor => Colors.GuiTypePhysics;
    public override string LabelIcon => Icons.Box;

    [RecordHistory] [JsonProperty] [Label("Size")] public Vector3 Size { get; set; } = Vector3.One;
    [RecordHistory] [JsonProperty] [Label("Center")] public Vector3 Center { get; set; } = Vector3.Zero;

    [JsonIgnore] public BoxShape? Shape;

    public override bool Load() {
        
        Obj.DecomposeWorldMatrix(out _, out _, out var scale);
        
        // Jitter2'de şekil boyutu yereldir, Rigidbody'ye eklenirken ölçeklendirilebilir 
        // veya burada direkt dünya ölçeğiyle oluşturulabilir.
        Shape = new BoxShape(Size.X * scale.X, Size.Y * scale.Y, Size.Z * scale.Z);
        return true;
    }

    public override void Loop(bool is2D) {
        
        if (is2D) return;

        if (!IsSelected || !CommandLine.Editor) return;
        
        // Sadece objenin dünya matrisini (pos, rot, scale) baz alıyoruz
        // Center ve Size'ı DrawCubeWires içinde local olarak kullanıyoruz
        Rlgl.PushMatrix();
        Rlgl.MultMatrixf(Obj.WorldMatrix);
        
        // DrawCubeWires parametreleri: Center (Local), Width, Height, Length, Color
        // Raylib DrawCubeWires merkezi baz alır, bu yüzden Center doğrudan çalışır.
        // Size değerini geçiyoruz, Obj.WorldMatrix'teki scale ile zaten çarpılacaktır.
        Raylib.DrawCubeWires(Center, Size.X, Size.Y, Size.Z, Raylib.ColorAlpha(Color.Green, 0.5f));
        
        Rlgl.PopMatrix();
    }
}
