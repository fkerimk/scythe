using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using Jitter2.Dynamics;
using Jitter2.Collision.Shapes;

internal class Rigidbody(Obj obj) : Component(obj) {

    public override Color LabelColor => Colors.GuiTypePhysics;
    public override string LabelIcon => Icons.Physics;

    [RecordHistory] [JsonProperty] [Label("Static")] public bool IsStatic { get; set; }
    [RecordHistory] [JsonProperty] [Label("Gravity")] public bool Gravity { get; set; } = true;

    [Header("Material")]
    [RecordHistory] [JsonProperty] [Label("Friction")] public float Friction { get; set; } = 0.5f;
    [RecordHistory] [JsonProperty] [Label("Bounciness")] public float Bounciness { get; set; } = 0.0f;

    [Header("Constraints")]
    [RecordHistory] [JsonProperty] [Label("Freeze Pos X")] public bool FreezePosX { get; set; }
    [RecordHistory] [JsonProperty] [Label("Freeze Pos Y")] public bool FreezePosY { get; set; }
    [RecordHistory] [JsonProperty] [Label("Freeze Pos Z")] public bool FreezePosZ { get; set; }
    [RecordHistory] [JsonProperty] [Label("Freeze Rot X")] public bool FreezeRotX { get; set; }
    [RecordHistory] [JsonProperty] [Label("Freeze Rot Y")] public bool FreezeRotY { get; set; }
    [RecordHistory] [JsonProperty] [Label("Freeze Rot Z")] public bool FreezeRotZ { get; set; }

    [JsonIgnore] public RigidBody? Body;

    private Vector3 _lastSyncedPos;
    private Quaternion _lastSyncedRot;

    public override bool Load() {
        if (CommandLine.Editor) return true;

        Body = Physics.World.CreateRigidBody();
        
        Obj.DecomposeWorldMatrix(out var pos, out var rot, out var scale);

        if (Obj.TryGetComponent<BoxCollider>(out var box)) {
            
            if (!box.IsLoaded) {
                box.Load();
                box.IsLoaded = true;
            }
            
            if (box.Shape != null) {
                if (box.Center != Vector3.Zero) {
                    var scaledCenter = box.Center * scale;
                    Body.AddShape(new TransformedShape(box.Shape, Conversion.ToJitter(scaledCenter)), false);
                } else {
                    Body.AddShape(box.Shape, false);
                }
            }
        }

        Body.Position = Conversion.ToJitter(pos);
        Body.Orientation = Conversion.ToJitter(rot);
        Body.MotionType = IsStatic ? MotionType.Static : MotionType.Dynamic;
        Body.AffectedByGravity = Gravity;
        Body.Friction = Friction;
        Body.Restitution = Bounciness;

        Body.SetMassInertia();

        _lastSyncedPos = pos;
        _lastSyncedRot = rot;

        return true;
    }

    public override void Loop(bool is2D) {
        
        if (is2D || CommandLine.Editor || Body == null) return;

        // Check for manual transform changes (Script or Teleport)
        if (Obj.Transform.Pos != _lastSyncedPos || Obj.Transform.Rot != _lastSyncedRot) {
            
            Body.Position = Conversion.ToJitter(Obj.Transform.Pos);
            Body.Orientation = Conversion.ToJitter(Obj.Transform.Rot);
            
            _lastSyncedPos = Obj.Transform.Pos;
            _lastSyncedRot = Obj.Transform.Rot;
        }
        
        if (Body.MotionType == MotionType.Dynamic) {
            
            // Constraints
            if (FreezePosX || FreezePosY || FreezePosZ) {
                
                var vel = Body.Velocity;
                if (FreezePosX) vel.X = 0;
                if (FreezePosY) vel.Y = 0;
                if (FreezePosZ) vel.Z = 0;
                Body.Velocity = vel;
            }

            if (FreezeRotX || FreezeRotY || FreezeRotZ) {
                
                var angVel = Body.AngularVelocity;
                if (FreezeRotX) angVel.X = 0;
                if (FreezeRotY) angVel.Y = 0;
                if (FreezeRotZ) angVel.Z = 0;
                Body.AngularVelocity = angVel;
            }

            // Sync physics to transform
            Obj.Transform.Pos = Conversion.FromJitter(Body.Position);
            Obj.Transform.Rot = Conversion.FromJitter(Body.Orientation);
            
            _lastSyncedPos = Obj.Transform.Pos;
            _lastSyncedRot = Obj.Transform.Rot;
        }
    }
}
