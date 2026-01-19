using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.ShaderLocationIndex;

internal static class Shaders {

    // Shaders
    public static Shader
        Transform,
        Pbr,
        Depth,
        Skybox;

    #region PBR information

    public static int 
        PbrLightCount,
        PbrMetallicValue,
        PbrRoughnessValue,
        PbrEmissiveIntensity,
        PbrEmissiveColor,
        PbrTiling,
        PbrAlphaCutoff,
        PbrLightVp,
        PbrShadowMap,
        PbrShadowLightIndex,
        PbrShadowStrength,
        PbrShadowMapResolution,
        PbrReceiveShadows;

    #endregion 
    
    public static unsafe void Init() {

        // Editor shaders
        Transform = Load("transform", vert: false);

        #region PBR shader
        
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
        PbrLightVp = GetShaderLocation(Pbr, "lightVP");
        PbrShadowMap = GetShaderLocation(Pbr, "shadowMap");
        PbrShadowLightIndex = GetShaderLocation(Pbr, "shadow_light_index");
        PbrShadowStrength = GetShaderLocation(Pbr, "shadow_strength");
        PbrShadowMapResolution = GetShaderLocation(Pbr, "shadow_map_resolution");
        PbrReceiveShadows = GetShaderLocation(Pbr, "receive_shadows");
        
        #endregion 
        
        // Depth shader
        Depth = Load("depth");
        
        // Skybox shader
        Skybox = Load("skybox");
        SetShaderValue(Skybox, GetShaderLocation(Skybox, "environmentMap"), (int)MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);
        SetShaderValue(Skybox, GetShaderLocation(Skybox, "doGamma"), 1, ShaderUniformDataType.Int);
        SetShaderValue(Skybox, GetShaderLocation(Skybox, "vflipped"), 0, ShaderUniformDataType.Int);
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
        UnloadShader(Pbr);
        UnloadShader(Depth);
        UnloadShader(Skybox);
    }
}