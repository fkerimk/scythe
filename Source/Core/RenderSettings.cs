using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

internal class RenderSettings {
    
    public RenderSettings(bool skipInitialization) {
        
        if (skipInitialization) return;
        
        AmbientIntensity = 1f;
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
            SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "ambient_color"), new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f), ShaderUniformDataType.Vec3);
        }
    }

    public static float ShadowFovScale;
}