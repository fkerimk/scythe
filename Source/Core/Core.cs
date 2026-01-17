using System.Numerics;
using Raylib_cs;

internal static class Core {

    public static Level? ActiveLevel;
    public static Camera3D? ActiveCamera;
    
    public static readonly Dictionary<int, Light> Lights = [];
    
    public static void Init() {

        // Ambient
        const float ambientIntensity = 1f;
        var ambientColor = new ScytheColor(1, 1, 1);
        
        // Shaders
        Shaders.Init();

        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_albedo"  ), CommandLine.Editor ? Config.Editor.PbrAlbedo   : Config.Runtime.PbrAlbedo , ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_normal"  ), CommandLine.Editor ? Config.Editor.PbrNormal   : Config.Runtime.PbrNormal, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_mra"     ), CommandLine.Editor ? Config.Editor.PbrMra      : Config.Runtime.PbrMra, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_emissive"), CommandLine.Editor ? Config.Editor.PbrEmissive : Config.Runtime.PbrEmissive, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "ambient_color"), ambientColor.to_vector4(), ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "ambient_intensity"), ambientIntensity, ShaderUniformDataType.Float);

        // Fonts
        Fonts.Init();
        
        // Level & camera
        ActiveLevel = new Level("Main");
        ActiveCamera = CommandLine.Editor ? new Camera3D() : ActiveLevel.FindType<Camera>()?.Cam;
    }

    public static void Load() {
        
        if (ActiveLevel == null) return;
        
        LoadObj(ActiveLevel.Root);
    }
    
    private static void LoadObj(Obj obj) {

        if (obj.Type?.IsLoaded != true)
            if (obj.Type?.Load() == true)
                obj.Type?.IsLoaded = true;

        obj.Children.Sort(ObjType.Comparer.Instance);
        
        foreach (var child in obj.Children)
            LoadObj(child);
    }

    public static unsafe void Loop3D() {

        if (ActiveLevel == null) return;

        Lights.Clear();
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.Pbr.Locs[(int)ShaderLocationIndex.VectorView], ActiveCamera?.Position ?? Vector3.Zero, ShaderUniformDataType.Vec3);
        
        Loop3DObj(ActiveLevel.Root);
        
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrLightCount, Lights.Count, ShaderUniformDataType.Int);
        
        foreach (var light in Lights.Values) light.Update();
    }
    
    private static void Loop3DObj(Obj obj) {
        
        if (obj.Parent != null) {
            
            obj.WorldMatrix = obj.Parent.WorldMatrix * obj.Matrix;
            obj.WorldRotMatrix = obj.Parent.WorldRotMatrix * obj.RotMatrix;
        }
        else {

            obj.WorldMatrix = obj.Matrix;
            obj.WorldRotMatrix = obj.RotMatrix;
        }
        
        obj.Type?.Loop3D();
        
        foreach (var child in obj.Children)
            Loop3DObj(child);
    }

    public static void LoopUi() {

        if (ActiveLevel == null) return;
        
        LoopUiObj(ActiveLevel.Root);
    }
    
    private static void LoopUiObj(Obj obj) {
        
        obj.Type?.LoopUi();
        
        foreach (var child in obj.Children)
            LoopUiObj(child);
    }

    public static void Loop3DEditor(Viewport viewport) {

        if (ActiveLevel == null) return;
        
        Loop3DEditorObj(ActiveLevel.Root, viewport);
    }
    
    private static void Loop3DEditorObj(Obj obj, Viewport viewport) {
        
        obj.Type?.Loop3DEditor(viewport);
        
        foreach (var child in obj.Children)
            Loop3DEditorObj(child, viewport);
    }
    
    public static void LoopUiEditor(Viewport viewport) {

        if (ActiveLevel == null) return;
        
        LoopUiEditorObj(ActiveLevel.Root, viewport);
    }
    
    private static void LoopUiEditorObj(Obj obj, Viewport viewport) {
        
        obj.Type?.LoopUiEditor(viewport);
        
        foreach (var child in obj.Children)
            LoopUiEditorObj(child, viewport);
    }

    public static void Quit() {
        
        Shaders.Quit();
        Fonts.UnloadRlFonts();

        if (ActiveLevel == null) return;
        
        QuitObj(ActiveLevel.Root);
    }

    private static void QuitObj(Obj obj) {
        
        obj.Type?.Quit();
        
        foreach (var child in obj.Children)
             QuitObj(child);
    }
}