using Raylib_cs;
using static Raylib_cs.Raylib;

internal class RenderSettings {
    
    public RenderSettings(bool skipInitialization) {
        
        if (skipInitialization) return;
        
        AmbientIntensity = 0.75f;
        AmbientColor = Color.White;
        
        ShadowFovScale = 1;
    }
    
    public static float AmbientIntensity {

        get; set {

            field = value;
            SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "ambient_intensity"), value, ShaderUniformDataType.Float);
        }
    }
    public static Color AmbientColor {

        get; set {

            field = value;
            SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "ambient_color"), value.ToVector4(), ShaderUniformDataType.Vec3);
        }
    }

    public static float ShadowFovScale;
}