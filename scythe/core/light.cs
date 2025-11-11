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
    
    private Vector3 pos = Vector3.Zero;
    private Vector3 target = Vector3.Zero;

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

        pos = _position;
        
        update();
        
        core.lights[GetHashCode()] = this;

        if ((!config.runtime.draw_lights || is_editor) && (!config.editor.draw_lights || !is_editor)) return;
        
        if (enabled) Raylib.DrawSphereEx(pos, 0.1f, 8, 8, color.to_raylib());
        else Raylib.DrawSphereWires(pos, 0.1f, 8, 8, Raylib.ColorAlpha(color.to_raylib(), 0.3f));
    }
    
    public override void loop_ui(bool is_editor) {}
    
    public override void loop_3d_editor(viewport viewport) { }
    public override void loop_ui_editor(viewport viewport) { }

    public override void quit() { }
}