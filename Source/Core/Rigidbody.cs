using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using Jitter2.Dynamics;
using Jitter2.Collision.Shapes;

internal class Rigidbody(Obj obj) : Component(obj) {

    public override Color LabelColor => Colors.GuiTypeObject;
    public override string LabelIcon => Icons.Obj;

    [RecordHistory] [JsonProperty] [Label("Static")] public bool IsStatic { get; set; }
    [RecordHistory] [JsonProperty] [Label("Affected By Gravity")] public bool AffectedByGravity { get; set; } = true;

    [JsonIgnore] public RigidBody? Body;

    public override bool Load() {
        if (CommandLine.Editor) return true;

        Body = Physics.World.CreateRigidBody();

        Obj.DecomposeWorldMatrix(out var pos, out var rot, out var scale);
        
        Body.Position = Conversion.ToJitter(pos);
        Body.Orientation = Conversion.ToJitter(rot);
        Body.MotionType = IsStatic ? MotionType.Static : MotionType.Dynamic;
        Body.AffectedByGravity = AffectedByGravity;

        if (Obj.TryGetComponent<BoxCollider>(out var box)) {
            
            if (!box.IsLoaded) {
                
                box.Load();
                box.IsLoaded = true;
            }
            
            if (box.Shape != null) {
                
                if (box.Center != Vector3.Zero) {
                    
                    var scaledCenter = box.Center * scale;
                    Body.AddShape(new TransformedShape(box.Shape, Conversion.ToJitter(scaledCenter)));
                }
                
                else Body.AddShape(box.Shape);
            }
        }

        return true;
    }

    public override void Loop(bool is2D) {
        
        if (is2D || CommandLine.Editor || Body == null) return;
        
        if (Body.MotionType == MotionType.Dynamic) {
            
            Obj.Transform.Pos = Conversion.FromJitter(Body.Position);
            Obj.Transform.Rot = Conversion.FromJitter(Body.Orientation);
        }
    }
}
