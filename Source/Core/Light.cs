using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

// ReSharper disable once ClassNeverInstantiated.Global
internal unsafe class Light(Obj obj) : ObjType(obj) {
    
    public override string LabelIcon => Icons.Light;
    public override Color LabelColor => Colors.GuiTypeLight;

    [JsonProperty] [Label("Enabled")] public bool Enabled { get; set; } = true;
    [JsonProperty] [Label("Type")] public int Type { get; set => field = (int)Raymath.Clamp(value, 0, 2); } = 1;
    [JsonProperty] [Label("Color")] public Color Color { get; set; } = Colors.White;
    [JsonProperty] [Label("Intensity")] public float Intensity { get; set; } = 2;
    [JsonProperty] [Label("Range")] public float Range { get; set; } = 10;
    
    private float3 _pos = float3.zero;
    private float3 _target = float3.zero;

    public void Update() {
        
        var enabledLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{Core.Lights.Count}].enabled");
        var typeLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{Core.Lights.Count}].type");
        var posLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{Core.Lights.Count}].position");
        var targetLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{Core.Lights.Count}].target");
        var colorLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{Core.Lights.Count}].color");
        var intensityLoc = Raylib.GetShaderLocation(Shaders.Pbr, $"lights[{Core.Lights.Count}].intensity");
        
        Raylib.SetShaderValue(Shaders.Pbr, enabledLoc, Enabled ? 1 : 0, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, typeLoc, Type, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, posLoc, _pos, ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(Shaders.Pbr, targetLoc, _target, ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(Shaders.Pbr, colorLoc, Color.to_vector4(), ShaderUniformDataType.Vec4);
        Raylib.SetShaderValue(Shaders.Pbr, intensityLoc, Intensity, ShaderUniformDataType.Float);
    }
    
    public override void Loop3D(bool isEditor) {

        if (Obj.Parent == null) return; 
        
        var position = Vector3.Zero;
        var rotation = Quaternion.Identity;
        var scale = Vector3.One;
    
        Raymath.MatrixDecompose( Obj.Parent.Matrix, &position, &rotation, &scale);

        _pos = position.to_float3();
        _target = _pos + Obj.Parent.Fwd * (Type == 0 ? 1 : Range);
        
        Update();
        
        Core.Lights[GetHashCode()] = this;

        if (IsSelected) {

            var gizmoColor = Raylib.ColorAlpha(Color.to_raylib(), 0.1f);
            
            switch (Type) {
                
                case 1: Raylib.DrawSphereWires(_pos.to_vector3(), Range, 8, 8, gizmoColor); break;
                
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

                        Raylib.DrawLine3D(point1.to_vector3(), point2.to_vector3(), gizmoColor);
                    }

                    // sides
                    const float sides = 4;
                    
                    for (var i = 0; i < sides; i++) {
                    
                        var angle = (i / sides) * MathF.PI * 2f;

                        var offset = Obj.Parent.Right * MathF.Cos(angle) * coneRadius + Obj.Parent.Up * MathF.Sin(angle) * coneRadius;
                        var point = baseCenter + offset;

                        Raylib.DrawLine3D(_pos.to_vector3(), point.to_vector3(), gizmoColor);
                    }

                    break;
                }
            }
        }
        
        if ((!Config.Runtime.DrawLights || isEditor) && (!Config.Editor.DrawLights || !isEditor)) return;

        Raylib.DrawSphereWires(_pos.to_vector3(), 0.1f, 8, 8, Enabled ? Raylib.ColorAlpha(Color.to_raylib(), 0.8f) : Raylib.ColorAlpha(Color.to_raylib(), 0.2f));
    }
    
    public override void LoopUi(bool isEditor) {}
    
    public override void Loop3DEditor(Viewport viewport) { }
    public override void LoopUiEditor(Viewport viewport) { }

    public override void Quit() { }
}