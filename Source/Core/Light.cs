using System.ComponentModel;
using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Light(Obj obj) : Component(obj, "light") {
    
    public override string LabelIcon => Icons.Light;
    public override ScytheColor LabelScytheColor => Colors.GuiTypeLight;

    [Label("Enabled"), JsonProperty, RecordHistory]
    public bool Enabled { get; set; } = true;
    
    [Label("Type"), JsonProperty, RecordHistory, DefaultValue(1)]
    public int Type { get; set => field = (int)Raymath.Clamp(value, 0, 2); } = 1;
    
    [Label("Color"), JsonProperty, RecordHistory]
    public ScytheColor ScytheColor { get; set; } = Colors.White;
    
    [Label("Intensity"), JsonProperty, RecordHistory, DefaultValue(2)]
    public float Intensity { get; set; } = 2;
    
    [Label("Range"), JsonProperty, RecordHistory, DefaultValue(10)]
    public float Range { get; set; } = 10;
    
    private Vector3 _pos = Vector3.Zero;
    private Vector3 _target = Vector3.Zero;

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
        Raylib.SetShaderValue(Shaders.Pbr, colorLoc, ScytheColor.to_vector4(), ShaderUniformDataType.Vec4);
        Raylib.SetShaderValue(Shaders.Pbr, intensityLoc, Intensity, ShaderUniformDataType.Float);
    }

    public override unsafe void Loop(bool is2D) {

        var position = Vector3.Zero;
        var rotation = Quaternion.Identity;
        var scale = Vector3.One;
    
        Raymath.MatrixDecompose( Obj.Matrix, &position, &rotation, &scale);

        _pos = position;
        _target = _pos + Obj.Fwd * (Type == 0 ? 1 : Range);
        
        Update();
        
        Core.Lights[GetHashCode()] = this;

        if (IsSelected) {

            var gizmoColor = Raylib.ColorAlpha(ScytheColor.ToRaylib(), 0.3f);
            
            switch (Type) {
                
                case 1: Raylib.DrawSphereWires(_pos, Range, 8, 8, gizmoColor); break;
                
                case 2: {
                    
                    var baseCenter = _pos + Obj.Fwd * Range;
                    var coneRadius = Range;

                    for (var i = 0; i < 8; i++) {
                    
                        var angle = (i / 8f) * MathF.PI * 2f;
                        var nextAngle = ((i + 1) / 8f) * MathF.PI * 2f;

                        var offset1 = Obj.Right * MathF.Cos(angle) * coneRadius + Obj.Up * MathF.Sin(angle) * coneRadius;
                        var offset2 = Obj.Right * MathF.Cos(nextAngle) * coneRadius + Obj.Up * MathF.Sin(nextAngle) * coneRadius;

                        var point1 = baseCenter + offset1;
                        var point2 = baseCenter + offset2;

                        Raylib.DrawLine3D(point1, point2, gizmoColor);
                    }

                    // sides
                    const float sides = 4;
                    
                    for (var i = 0; i < sides; i++) {
                    
                        var angle = (i / sides) * MathF.PI * 2f;

                        var offset = Obj.Right * MathF.Cos(angle) * coneRadius + Obj.Up * MathF.Sin(angle) * coneRadius;
                        var point = baseCenter + offset;

                        Raylib.DrawLine3D(_pos, point, gizmoColor);
                    }

                    break;
                }
            }
        }
        
        if ((!Config.Runtime.DrawLights || CommandLine.Editor) && (!Config.Editor.DrawLights || !CommandLine.Editor)) return;

        Raylib.DrawSphereWires(_pos, 0.1f, 8, 8, Enabled ? Raylib.ColorAlpha(ScytheColor.ToRaylib(), 0.8f) : Raylib.ColorAlpha(ScytheColor.ToRaylib(), 0.2f));
    }
}