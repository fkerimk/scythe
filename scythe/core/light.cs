using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public unsafe class light(obj obj) : type(obj) {
    
    public override string label_icon => icons.light;
    public override color label_color => colors.gui_type_light;

    [label("Enabled")] public bool enabled { get; set; } = true;
    [label("Type")] public int type { get; set => field = (int)Raymath.Clamp(value, 0, 2); } = 1;
    [label("Color")] public color color { get; set; } = colors.white;
    [label("Intensity")] public float intensity { get; set; } = 2;
    [label("Range")] public float range { get; set; } = 10;
    
    private float3 pos = float3.zero;
    private float3 target = float3.zero;

    public void update() {
        
        var enabled_loc = Raylib.GetShaderLocation(shaders.pbr, $"lights[{core.lights.Count}].enabled");
        var type_loc = Raylib.GetShaderLocation(shaders.pbr, $"lights[{core.lights.Count}].type");
        var pos_loc = Raylib.GetShaderLocation(shaders.pbr, $"lights[{core.lights.Count}].position");
        var target_loc = Raylib.GetShaderLocation(shaders.pbr, $"lights[{core.lights.Count}].target");
        var color_loc = Raylib.GetShaderLocation(shaders.pbr, $"lights[{core.lights.Count}].color");
        var intensity_loc = Raylib.GetShaderLocation(shaders.pbr, $"lights[{core.lights.Count}].intensity");
        
        Raylib.SetShaderValue(shaders.pbr, enabled_loc, enabled ? 1 : 0, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(shaders.pbr, type_loc, type, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(shaders.pbr, pos_loc, pos, ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(shaders.pbr, target_loc, target, ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(shaders.pbr, color_loc, color.to_vector4(), ShaderUniformDataType.Vec4);
        Raylib.SetShaderValue(shaders.pbr, intensity_loc, intensity, ShaderUniformDataType.Float);
    }
    
    public override void loop_3d(bool is_editor) {

        if (obj.parent == null) return; 
        
        var _position = Vector3.Zero;
        var _rotation = Quaternion.Identity;
        var _scale = Vector3.One;
    
        Raymath.MatrixDecompose( obj.parent.matrix, &_position, &_rotation, &_scale);

        pos = _position.to_float3();
        target = pos + obj.parent.fwd * (type == 0 ? 1 : range);
        
        //var a = pos + (Vector3.Normalize(obj.fwd.to_vector3()) * 0.1f).to_float3();
        //var b = a + obj.fwd * 1.5f;
        
        update();
        
        core.lights[GetHashCode()] = this;

        if (is_selected) {
            
            if (type == 1) Raylib.DrawSphereWires(pos.to_vector3(), range, 8, 8, color.to_raylib());
            
            else if (type == 2) {

                float sides = 4;
    
                var base_center = pos + obj.parent.fwd * range;
                var cone_radius = range;

                // Cone tabanı
                for (var i = 0; i < 8; i++) {
                    
                    var angle = (i / 8f) * MathF.PI * 2f;
                    var next_angle = ((i + 1) / 8f) * MathF.PI * 2f;

                    var offset1 = obj.parent.right * MathF.Cos(angle) * cone_radius + obj.parent.up * MathF.Sin(angle) * cone_radius;
                    var offset2 = obj.parent.right * MathF.Cos(next_angle) * cone_radius + obj.parent.up * MathF.Sin(next_angle) * cone_radius;

                    var point1 = base_center + offset1;
                    var point2 = base_center + offset2;

                    Raylib.DrawLine3D(point1.to_vector3(), point2.to_vector3(), color.to_raylib());
                }

                // Cone kenarları
                for (var i = 0; i < sides; i++) {
                    
                    var angle = (i / sides) * MathF.PI * 2f;

                    var offset = obj.parent.right * MathF.Cos(angle) * cone_radius + obj.parent.up * MathF.Sin(angle) * cone_radius;
                    var point = base_center + offset;

                    Raylib.DrawLine3D(pos.to_vector3(), point.to_vector3(), color.to_raylib());
                }
            }
        }
        
        if ((!config.runtime.draw_lights || is_editor) && (!config.editor.draw_lights || !is_editor)) return;
        
        if (enabled) Raylib.DrawSphereEx(pos.to_vector3(), 0.1f, 8, 8, color.to_raylib());
        else Raylib.DrawSphereWires(pos.to_vector3(), 0.1f, 8, 8, Raylib.ColorAlpha(color.to_raylib(), 0.3f));
    }
    
    public override void loop_ui(bool is_editor) {}
    
    public override void loop_3d_editor(viewport viewport) { }
    public override void loop_ui_editor(viewport viewport) { }

    public override void quit() { }
}