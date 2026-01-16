using Raylib_cs;

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

    public static void Begin(Shader shader) {
        
        Raylib.BeginShaderMode(shader);
    }
    
    public static void End () {
        
        Raylib.EndShaderMode();
    }
    
    public static unsafe void Init() {

        // load generic shaders
        Transform = Load("transform", vert: false);
        
        // load pbr shader
        Pbr = Load("pbr");
        Pbr.Locs[(int)ShaderLocationIndex.MapAlbedo] = Raylib.GetShaderLocation(Pbr, "albedo_map");
        Pbr.Locs[(int)ShaderLocationIndex.MapMetalness] = Raylib.GetShaderLocation(Pbr, "mra_map");
        Pbr.Locs[(int)ShaderLocationIndex.MapNormal] = Raylib.GetShaderLocation(Pbr, "normal_map");
        Pbr.Locs[(int)ShaderLocationIndex.MapEmission] = Raylib.GetShaderLocation(Pbr, "emissive_map");
        Pbr.Locs[(int)ShaderLocationIndex.ColorDiffuse] = Raylib.GetShaderLocation(Pbr, "albedo_color");
        Pbr.Locs[(int)ShaderLocationIndex.VectorView] = Raylib.GetShaderLocation(Pbr, "view_pos");
        PbrLightCount = Raylib.GetShaderLocation(Pbr, "light_count");
        PbrMetallicValue = Raylib.GetShaderLocation(Pbr, "metallic_value");
        PbrEmissiveIntensity = Raylib.GetShaderLocation(Pbr, "emissive_intensity");
        PbrRoughnessValue = Raylib.GetShaderLocation(Pbr, "roughness_value");
        PbrEmissiveColor = Raylib.GetShaderLocation(Pbr, "emissive_color");
        PbrTiling = Raylib.GetShaderLocation(Pbr, "tiling");
    }

    private static Shader Load(string shader, bool vert = true, bool frag = true) {

        string? vsPath = null, fsPath = null;
        
        if ((vert && !PathUtil.BestPath(Path.Combine("Shaders", shader + ".vs"), out vsPath)) ||
            (frag && !PathUtil.BestPath(Path.Combine("Shaders", shader + ".fs"), out fsPath)))
            throw new FileNotFoundException("Shader not found", shader);

        return Raylib.LoadShader(vsPath, fsPath);
    }

    public static void Quit() {
        
        Raylib.UnloadShader(Transform);
    }
}