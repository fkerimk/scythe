using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.ShaderLocationIndex;

#pragma warning disable CS8981
internal static class Shaders {

    public static Shader Transform;
    public static Shader Pbr;

    public static int PbrLightCount;
    public static int PbrMetallicValue;
    public static int PbrRoughnessValue;
    public static int PbrEmissiveIntensity;
    public static int PbrEmissiveColor;
    public static int PbrTiling;
    public static int PbrAlphaCutoff;

    public static void Begin(Shader shader) {
        
        BeginShaderMode(shader);
    }
    
    public static void End () {
        
        EndShaderMode();
    }
    
    public static unsafe void Init() {

        // load generic shaders
        Transform = Load("transform", vert: false);
        
        // load pbr shader
        Pbr = Load("pbr");
        Pbr.Locs[(int)MapAlbedo] = GetShaderLocation(Pbr, "albedo_map");
        Pbr.Locs[(int)MapMetalness] = GetShaderLocation(Pbr, "mra_map");
        Pbr.Locs[(int)MapNormal] = GetShaderLocation(Pbr, "normal_map");
        Pbr.Locs[(int)MapEmission] = GetShaderLocation(Pbr, "emissive_map");
        Pbr.Locs[(int)ColorDiffuse] = GetShaderLocation(Pbr, "albedo_color");
        Pbr.Locs[(int)VectorView] = GetShaderLocation(Pbr, "view_pos");
        PbrLightCount = GetShaderLocation(Pbr, "light_count");
        PbrMetallicValue = GetShaderLocation(Pbr, "metallic_value");
        PbrEmissiveIntensity = GetShaderLocation(Pbr, "emissive_intensity");
        PbrRoughnessValue = GetShaderLocation(Pbr, "roughness_value");
        PbrEmissiveColor = GetShaderLocation(Pbr, "emissive_color");
        PbrTiling = GetShaderLocation(Pbr, "tiling");
        PbrAlphaCutoff = GetShaderLocation(Pbr, "alpha_cutoff");
    }

    private static Shader Load(string shader, bool vert = true, bool frag = true) {

        string? vsPath = null, fsPath = null;
        
        if ((vert && !PathUtil.BestPath(Path.Combine("Shaders", shader + ".vs"), out vsPath)) ||
            (frag && !PathUtil.BestPath(Path.Combine("Shaders", shader + ".fs"), out fsPath)))
            throw new FileNotFoundException("Shader not found", shader);

        return LoadShader(vsPath, fsPath);
    }

    public static void Quit() {
        
        UnloadShader(Transform);
    }
}