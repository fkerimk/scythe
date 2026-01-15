using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable CS8981
internal class Transform(Obj obj) : ObjType(obj) {
    
    public override int Priority => 0;

    public override string LabelIcon => Icons.Transform;
    public override Color LabelColor => Colors.GuiTypeTransform;
    
    [RecordHistory] [JsonProperty] [Label("Pos")] public float3 Pos { get; set; } = float3.zero;
    
    [RecordHistory] [JsonProperty] [Label("Euler")] public float3 Euler { 
        
        get => Raymath.QuaternionToEuler(Rot).to_float3().to_deg();
        set => Rot = Raymath.QuaternionFromEuler(value.x.DegToRad(), value.y.DegToRad(), value.z.DegToRad());
    }
    
    [RecordHistory] [JsonProperty] [Label("Scale")] public float3 Scale { get; set; } = float3.one;
    
    [RecordHistory] [JsonProperty] private Quaternion Rot { get; set; } = Quaternion.Identity;

    private int _mode;
    private float _activeMove;
    
    private string _activeId = "";
    private Vector2 _activeMouseTemp;
    private float3 _activePos;
    private float3 _activeScale;
    private Quaternion _activeRot = Quaternion.Identity;
    private float3 _activeNormal;

    private const float MoveSnap = 0.2f;

    private bool _canUseShortcuts;

    public override bool Load(Core core, bool isEditor) => true;

    public override void Loop3D(Core core, bool isEditor) {

        if (Obj.Parent == null) return;
        
        Obj.Parent.RotMatrix = Matrix4x4.CreateFromQuaternion(Rot);
        
        Obj.Parent.Matrix = Raymath.MatrixMultiply(

            Raymath.MatrixMultiply(
    
                Raymath.MatrixScale(Scale.x, Scale.y, Scale.z),
                Matrix4x4.Transpose(Obj.Parent.RotMatrix)
            ),

            Raymath.MatrixTranslate(Pos.x, Pos.y, Pos.z)
        );
        
    }

    public override void LoopUi(Core core, bool isEditor) {}

    public override void Loop3DEditor(Core core, Viewport viewport) {
        
        if (core.ActiveCamera == null||
            viewport is not Level3D level3d
        ) return;

        if (Obj.Parent == null || (!Obj.Parent.IsSelected && !Obj.IsSelected && !Obj.Parent.Children.Any(o => o.IsSelected))) return;
        
        if (_canUseShortcuts && _activeMove == 0) {
        
            if (Raylib.IsKeyPressed(KeyboardKey.Q)) _mode = 0;
            if (Raylib.IsKeyPressed(KeyboardKey.W)) _mode = 1;
            if (Raylib.IsKeyPressed(KeyboardKey.E)) _mode = 2;
        }
        
        Shaders.Begin(Shaders.Transform);

        var ray = Raylib.GetScreenToWorldRay(level3d.RelativeMouse3D, core.ActiveCamera.RlCam);
        //Raylib.DrawSphere(ray.Position + ray.Direction * 15, 0.1f, Color.Magenta);
        
        Axis(core, "x", new float3(1, 0, 0), Obj.Parent.Right, new Color(0.9f, 0.3f, 0.3f), ray);
        Axis(core, "y", new float3(0, 1, 0), Obj.Parent.Up, new Color(0.3f, 0.9f, 0.3f), ray);
        Axis(core, "z", new float3(0, 0, 1), Obj.Parent.Fwd, new Color(0.3f, 0.3f, 0.9f), ray);
        
        Shaders.End();
    }

    public override void LoopUiEditor(Core core, Viewport viewport) {

        _canUseShortcuts = false;
        
        if (Raylib.IsCursorHidden()) return;
        
        if (Obj.Parent == null || (!Obj.Parent.IsSelected && !Obj.IsSelected && !Obj.Parent.Children.Any(o => o.IsSelected))) return;

        _canUseShortcuts = true;
        
        var textA = _mode switch { 0 => "pos", 1 => "rot", 2 => "scale", _ => "bruh" };
        var textPosA = new int2(viewport.RelativeMouse.X, viewport.RelativeMouse.Y - 15);
        
        Raylib.DrawText(textA, textPosA.x - 14, textPosA.y - 19, 20, Colors.Black.ToRaylib());
        Raylib.DrawText(textA, textPosA.x - 15, textPosA.y - 20, 20, Colors.Yellow.ToRaylib());
        
        if (_activeMove == 0) return;
        
        var textB = _mode switch { 0 or 2 => $"{_activeMove:F2}m", 1 => $"{_activeMove:F2}°", _ => $"{_activeMove:F2}" }; 
        var textPosB = new int2(viewport.RelativeMouse.X, viewport.RelativeMouse.Y - 15);
            
        Raylib.DrawText(textB, textPosB.x - 14, textPosB.y - 39, 20, Colors.Black.ToRaylib());
        Raylib.DrawText(textB, textPosB.x - 15, textPosB.y - 40, 20, Colors.Yellow.ToRaylib());
    }

    private void Axis(Core core, string id, float3 axis, float3 normal, Color axisColor, Ray ray) {

        var isActive = _activeId == id;
        
        var a = Pos + (Vector3.Normalize(normal.to_vector3()) * 0.1f).to_float3();
        var b = a + normal * 1.5f;

        if (!string.IsNullOrEmpty(_activeId) && _activeId != id) {
            
            Raylib.DrawLine3D(a.to_vector3(), b.to_vector3(), axisColor.ToRaylib());
            return;
        }

        if (isActive) {

            var newPos = Pos;
            var newRot = Rot;
            var newScale = Scale;

            var diff = (Raylib.GetMousePosition() - _activeMouseTemp) * _mode switch { 0 => 0.01f, 1 => 1f, 2 => 0.05f, _ => 0 };
                
            var drag = _mode switch {
                
                0 or 2 => core.ActiveCamera.Right * diff.X + core.ActiveCamera.Up * -diff.Y,
                1 => core.ActiveCamera.Up * diff.X + core.ActiveCamera.Right * diff.Y,
                _ => float3.zero
            };

            var camDistance = Vector3.Distance(_activePos.to_vector3(), core.ActiveCamera.Pos.to_vector3());
            var move = Vector3.Dot(drag.to_vector3(), _activeNormal.to_vector3()) * camDistance * 0.25f;

            _activeMove = _mode switch {
                    
                0 when !Raylib.IsKeyDown(KeyboardKey.LeftShift) => MathF.Round(move / MoveSnap) * MoveSnap,
                1 when !Raylib.IsKeyDown(KeyboardKey.LeftShift) => MathF.Round(move / 22.5f) * 22.5f,
                2 when !Raylib.IsKeyDown(KeyboardKey.LeftShift) => MathF.Round(move / MoveSnap) * MoveSnap,
                _ => move
            };
            
            switch (_mode) {
                
                case 0: newPos = _activePos + _activeNormal * _activeMove; break;
                
                case 1:
                    var normalQ = Quaternion.CreateFromAxisAngle(_activeNormal.to_vector3(), _activeMove.DegToRad());
                    newRot = normalQ * _activeRot;
                    break;
                
                case 2: newScale = _activeScale + _activeNormal * _activeMove; break;
            }
            
            Pos = newPos;
            Rot = newRot;
            Scale = newScale;
        }
        
        const float radius = 0.025f, rayRadius = 0.125f;
        const int rayQuality = 9;
        
        var isHovered = false;
        
        for (var i = 0; i < rayQuality + 1; i++) {

            var step = Vector3.Lerp(a.to_vector3(), b.to_vector3(), 1f / rayQuality * i).to_float3();

            //Raylib.DrawSphere(step.to_vector3(), ray_radius, axis_color.to_raylib());
            if (Raylib.GetRayCollisionSphere(ray, step.to_vector3(), rayRadius).Hit)
                isHovered = true;
        }
        
        if (isHovered && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
            
            _activeId = id;

            _activePos = Pos;
            _activeRot = Rot;
            _activeScale = Scale;
    
            _activeMouseTemp = Raylib.GetMousePosition();

            _activeNormal = _mode switch { 2 => axis, _ => normal };
            
            History.StartRecording(this, "Transform");
        }

        if ((isActive && Raylib.IsMouseButtonReleased(MouseButton.Left)) || Raylib.IsCursorHidden()) {
            
            _activeId = "";
            _activeMove = 0;
            
            History.StopRecording();
        }

        var targetColor = (!isActive && isHovered && !Raylib.IsCursorHidden()) ? Colors.White : axisColor;
        
        Raylib.DrawCylinderEx(a.to_vector3(), b.to_vector3(), radius, radius, 1, targetColor.ToRaylib());
    }
    
    public override void Quit() {}
}