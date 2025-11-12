using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public static class shaders {

    public static Shader transform;
    public static Shader pbr;

    public static int pbr_light_count;
    public static int pbr_metallic_value;
    public static int pbr_roughness_value;
    public static int pbr_emissive_intensity;
    public static int pbr_emissive_color;
    public static int pbr_tiling;

    public static void begin(Shader shader) {
        
        Raylib.BeginShaderMode(shader);
    }
    
    public static void end () {
        
        Raylib.EndShaderMode();
    }
    
    public static unsafe void init() {

        // load generic shaders
        transform = load("transform");
        
        // load pbr shader
        pbr = load("pbr");
        pbr.Locs[(int)ShaderLocationIndex.MapAlbedo] = Raylib.GetShaderLocation(pbr, "albedo_map");
        pbr.Locs[(int)ShaderLocationIndex.MapMetalness] = Raylib.GetShaderLocation(pbr, "mra_map");
        pbr.Locs[(int)ShaderLocationIndex.MapNormal] = Raylib.GetShaderLocation(pbr, "normal_map");
        pbr.Locs[(int)ShaderLocationIndex.MapEmission] = Raylib.GetShaderLocation(pbr, "emissive_map");
        pbr.Locs[(int)ShaderLocationIndex.ColorDiffuse] = Raylib.GetShaderLocation(pbr, "albedo_color");
        pbr.Locs[(int)ShaderLocationIndex.VectorView] = Raylib.GetShaderLocation(pbr, "view_pos");
        pbr_light_count = Raylib.GetShaderLocation(pbr, "light_count");
        pbr_metallic_value = Raylib.GetShaderLocation(pbr, "metallic_value");
        pbr_emissive_intensity = Raylib.GetShaderLocation(pbr, "emissive_intensity");
        pbr_roughness_value = Raylib.GetShaderLocation(pbr, "roughness_value");
        pbr_emissive_color = Raylib.GetShaderLocation(pbr, "emissive_color");
        pbr_tiling = Raylib.GetShaderLocation(pbr, "tiling");
    }

    public static Shader load(string shader) {

        var vs_path = path.relative(Path.Combine("shader", shader + ".vs"));
        var fs_path = path.relative(Path.Combine("shader", shader + ".fs"));

        if (!File.Exists(vs_path)) vs_path = null;
        if (!File.Exists(fs_path)) fs_path = null;

        return Raylib.LoadShader(vs_path, fs_path);
    }

    public static void quit() {
        
        Raylib.UnloadShader(transform);
    }
}