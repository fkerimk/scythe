using System.ComponentModel;
using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

// ReSharper disable once ClassNeverInstantiated.Global
internal unsafe class Light(Obj obj) : ObjType(obj) {
    
    public override string LabelIcon => Icons.Light;
    public override Color LabelColor => Colors.GuiTypeLight;

    [RecordHistory] [JsonProperty] [Label("Enabled")] public bool Enabled { get; set; } = true;
    [RecordHistory] [JsonProperty] [Label("Type")] [DefaultValue(1)] public int Type { get; set => field = (int)Raymath.Clamp(value, 0, 2); }
    [RecordHistory] [JsonProperty] [Label("Color")] public Color Color { get; set; } = Colors.White;
    [RecordHistory] [JsonProperty] [Label("Intensity")] [DefaultValue(2)] public float Intensity { get; set; }
    [RecordHistory] [JsonProperty] [Label("Range")] [DefaultValue(10)] public float Range { get; set; }
    
    private Vector3 _pos = Vector3.Zero;
    private Vector3 _target = Vector3.Zero;

    public void Update(Core core) {
        
        var enabledLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{core.Lights.Count}].enabled");
        var typeLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{core.Lights.Count}].type");
        var posLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{core.Lights.Count}].position");
        var targetLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{core.Lights.Count}].target");
        var colorLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{core.Lights.Count}].color");
        var intensityLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{core.Lights.Count}].intensity");
        
        Raylib.SetShaderValue(Shaders.Pbr, enabledLoc, Enabled ? 1 : 0, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, typeLoc, Type, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, posLoc, _pos, ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(Shaders.Pbr, targetLoc, _target, ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(Shaders.Pbr, colorLoc, Color.to_vector4(), ShaderUniformDataType.Vec4);
        Raylib.SetShaderValue(Shaders.Pbr, intensityLoc, Intensity, ShaderUniformDataType.Float);
    }

    public override bool Load(Core core, bool isEditor) => true;

    public override void Loop3D(Core core, bool isEditor) {

        if (Obj.Parent == null) return; 
        
        var position = Vector3.Zero;
        var rotation = Quaternion.Identity;
        var scale = Vector3.One;
    
        Raymath.MatrixDecompose( Obj.Parent.Matrix, &position, &rotation, &scale);

        _pos = position;
        _target = _pos + Obj.Parent.Fwd * (Type == 0 ? 1 : Range);
        
        Update(core);
        
        core.Lights[GetHashCode()] = this;

        if (IsSelected) {

            var gizmoColor = Raylib.ColorAlpha(Color.ToRaylib(), 0.3f);
            
            switch (Type) {
                
                case 1: Raylib.DrawSphereWires(_pos, Range, 8, 8, gizmoColor); break;
                
                case 2: {
                    
                    var baseCenter = _pos + Obj.Parent.Fwd * Range;
                    var coneRadius = Range;

                    for (var i = 0; i < 8; i++) {
                    
                        var angle = (i / 8f) * MathF.PI * 2f;
                        var nextAngle = ((i + 1) / 8f) * MathF.PI * 2f;

                        var offset1 = Obj.Parent.Right * MathF.Cos(angle) * coneRadius + Obj.Parent.Up * MathF.Sin(angle) * coneRadius;
                        var offset2 = Obj.Parent.Right * MathF.Cos(nextAngle) * coneRadius + Obj.Parent.Up * MathF.Sin(nextAngle) * coneRadius;

                        var point1 = baseCenter + offset1;
                        var point2 = baseCenter + offset2;

                        Raylib.DrawLine3D(point1, point2, gizmoColor);
                    }

                    // sides
                    const float sides = 4;
                    
                    for (var i = 0; i < sides; i++) {
                    
                        var angle = (i / sides) * MathF.PI * 2f;

                        var offset = Obj.Parent.Right * MathF.Cos(angle) * coneRadius + Obj.Parent.Up * MathF.Sin(angle) * coneRadius;
                        var point = baseCenter + offset;

                        Raylib.DrawLine3D(_pos, point, gizmoColor);
                    }

                    break;
                }
            }
        }
        
        if ((!Config.Runtime.DrawLights || isEditor) && (!Config.Editor.DrawLights || !isEditor)) return;

        Raylib.DrawSphereWires(_pos, 0.1f, 8, 8, Enabled ? Raylib.ColorAlpha(Color.ToRaylib(), 0.8f) : Raylib.ColorAlpha(Color.ToRaylib(), 0.2f));
    }
    
    public override void LoopUi(Core core, bool isEditor) {}
    
    public override void Loop3DEditor(Core core, Viewport viewport) { }
    public override void LoopUiEditor(Core core, Viewport viewport) { }

    public override void Quit() { }
}