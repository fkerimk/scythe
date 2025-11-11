using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public static class shaders {

    public static Shader transform;
    public static Shader pbr;

    public static int light_count_loc;
    public static int metallicValueLoc;
    public static int roughnessValueLoc;
    public static int emissiveIntensityLoc;
    public static int emissiveColorLoc;
    public static int textureTilingLoc;

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
        pbr.Locs[(int)ShaderLocationIndex.MapAlbedo] = Raylib.GetShaderLocation(pbr, "albedoMap");
        pbr.Locs[(int)ShaderLocationIndex.MapMetalness] = Raylib.GetShaderLocation(pbr, "mraMap");
        pbr.Locs[(int)ShaderLocationIndex.MapNormal] = Raylib.GetShaderLocation(pbr, "normalMap");
        pbr.Locs[(int)ShaderLocationIndex.MapEmission] = Raylib.GetShaderLocation(pbr, "emissiveMap");
        pbr.Locs[(int)ShaderLocationIndex.ColorDiffuse] = Raylib.GetShaderLocation(pbr, "albedoColor");
        pbr.Locs[(int)ShaderLocationIndex.VectorView] = Raylib.GetShaderLocation(pbr, "viewPos");
        light_count_loc = Raylib.GetShaderLocation(pbr, "numOfLights");
        metallicValueLoc = Raylib.GetShaderLocation(pbr, "metallicValue");
        roughnessValueLoc = Raylib.GetShaderLocation(pbr, "roughnessValue");
        emissiveIntensityLoc = Raylib.GetShaderLocation(pbr, "emissivePower");
        emissiveColorLoc = Raylib.GetShaderLocation(pbr, "emissiveColor");
        textureTilingLoc = Raylib.GetShaderLocation(pbr, "tiling");
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