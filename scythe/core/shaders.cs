using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public static class shaders {

    private static Shader transform; public static void begin_transform() { Raylib.BeginShaderMode(transform); }

    public static void end () {
        
        Raylib.EndShaderMode();
    }
    
    public static void load() {

        transform = Raylib.LoadShader(null, path.relative("shader/transform.fs"));
    }

    public static void unload() {
        
        Raylib.UnloadShader(transform);
    }
}