using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public static class core {

    public static level? level;
    
    public static Dictionary<int, light> lights = [];
    
    public static void init(bool is_editor) {

        level = new();
        cam.main = new();

        lights = [];

        shaders.init();
        
        const float ambient_intensity = 0.02f;
        var ambient_color = new color(1, 1, 1);

        Raylib.SetShaderValue(shaders.pbr, Raylib.GetShaderLocation(shaders.pbr, "use_tex_albedo"  ), is_editor ? config.editor.pbr_albedo   : config.runtime.pbr_albedo , ShaderUniformDataType.Int);
        Raylib.SetShaderValue(shaders.pbr, Raylib.GetShaderLocation(shaders.pbr, "use_tex_normal"  ), is_editor ? config.editor.pbr_normal   : config.runtime.pbr_normal, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(shaders.pbr, Raylib.GetShaderLocation(shaders.pbr, "use_tex_mra"     ), is_editor ? config.editor.pbr_mra      : config.runtime.pbr_mra, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(shaders.pbr, Raylib.GetShaderLocation(shaders.pbr, "use_tex_emissive"), is_editor ? config.editor.pbr_emissive : config.runtime.pbr_emissive, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(shaders.pbr, Raylib.GetShaderLocation(shaders.pbr, "ambient_color"), ambient_color.to_vector4(), ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(shaders.pbr, Raylib.GetShaderLocation(shaders.pbr, "ambient_intensity"), ambient_intensity, ShaderUniformDataType.Float);
    }

    public static unsafe void loop_3d(bool is_editor) {

        if (level == null) return;
        if (cam.main == null) return;
        
        lights.Clear();
        Raylib.SetShaderValue(shaders.pbr, shaders.pbr.Locs[(int)ShaderLocationIndex.VectorView], cam.main.pos, ShaderUniformDataType.Vec3);
        
        loop_3d_obj(level.root, is_editor);
        
        Raylib.SetShaderValue(shaders.pbr, shaders.pbr_light_count, lights.Count, ShaderUniformDataType.Int);
        
        foreach (var light in lights.Values) light.update();
    }
    
    private static void loop_3d_obj(obj obj, bool is_editor, int index = 0) {
        
        obj.matrix = Matrix4x4.Identity;
        obj.type?.loop_3d(is_editor);

        foreach (var priority in new[] { 0, 1, 2, 3, 4 }) {
            
            foreach (var child in from child in obj.children let sortOrder = child.type switch {
                         
                transform => 0,
                animation => 1,
                model => 2,
                _ => 4

            } where sortOrder == priority select child) loop_3d_obj(child, is_editor, index + 1);
        }
    }

    public static void loop_ui(bool is_editor) {

        if (level == null) return;
        
        loop_ui_obj(level.root, is_editor);
    }
    
    private static void loop_ui_obj(obj obj, bool is_editor, int index = 0) {
        
        obj.type?.loop_ui(is_editor);
        
        foreach (var child in obj.children)
            loop_ui_obj(child, is_editor, index + 1);
    }

    public static void loop_3d_editor(viewport viewport) {

        if (level == null) return;
        
        loop_3d_editor_obj(level.root, viewport);
    }
    
    private static void loop_3d_editor_obj(obj obj, viewport viewport, int index = 0) {
        
        obj.type?.loop_3d_editor(viewport);
        
        foreach (var child in obj.children)
            loop_3d_editor_obj(child, viewport, index + 1);
    }
    
    public static void loop_ui_editor(viewport viewport) {

        if (level == null) return;
        
        loop_ui_editor_obj(level.root, viewport);
    }
    
    private static void loop_ui_editor_obj(obj obj, viewport viewport, int index = 0) {
        
        obj.type?.loop_ui_editor(viewport);
        
        foreach (var child in obj.children)
            loop_ui_editor_obj(child, viewport, index + 1);
    }

    public static void quit() {
        
        shaders.quit();

        if (level == null) return;
        
        quit_obj(level.root);
    }

    private static void quit_obj(obj obj, int index = 0) {
        
        obj.type?.quit();
        
        foreach (var child in obj.children)
             quit_obj(child, index + 1);
    }
}