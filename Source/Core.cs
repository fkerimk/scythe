using System.Numerics;
using Raylib_cs;

internal static class Core {

    public static Level? ActiveLevel;
    public static Dictionary<int, Light> Lights = [];
    
    public static void Init(bool isEditor) {

        ActiveLevel = new Level("Main");
        Cam.Main = new Cam();

        Lights = [];

        Shaders.Init();
        Fonts.LoadRlFonts();
        
        const float ambientIntensity = 0.02f;
        var ambientColor = new Color(1, 1, 1);

        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_albedo"  ), isEditor ? Config.Editor.PbrAlbedo   : Config.Runtime.PbrAlbedo , ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_normal"  ), isEditor ? Config.Editor.PbrNormal   : Config.Runtime.PbrNormal, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_mra"     ), isEditor ? Config.Editor.PbrMra      : Config.Runtime.PbrMra, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_emissive"), isEditor ? Config.Editor.PbrEmissive : Config.Runtime.PbrEmissive, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "ambient_color"), ambientColor.to_vector4(), ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "ambient_intensity"), ambientIntensity, ShaderUniformDataType.Float);
    }

    public static unsafe void Loop3D(bool isEditor) {

        if (ActiveLevel == null) return;
        if (Cam.Main == null) return;
        
        Lights.Clear();
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.Pbr.Locs[(int)ShaderLocationIndex.VectorView], Cam.Main.Pos, ShaderUniformDataType.Vec3);
        
        Loop3DObj(ActiveLevel.Root, isEditor);
        
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrLightCount, Lights.Count, ShaderUniformDataType.Int);
        
        foreach (var light in Lights.Values) light.Update();
    }
    
    private static void Loop3DObj(Obj obj, bool isEditor, int index = 0) {
        
        obj.Matrix = Matrix4x4.Identity;
        obj.Type?.Loop3D(isEditor);

        foreach (var priority in new[] { 0, 1, 2, 3, 4 }) {
            
            foreach (var child in from child in obj.Children let sortOrder = child.Type switch {
                         
                Transform => 0,
                Animation => 1,
                Model => 2,
                _ => 4

            } where sortOrder == priority select child) Loop3DObj(child, isEditor, index + 1);
        }
    }

    public static void LoopUi(bool isEditor) {

        if (ActiveLevel == null) return;
        
        LoopUiObj(ActiveLevel.Root, isEditor);
    }
    
    private static void LoopUiObj(Obj obj, bool isEditor, int index = 0) {
        
        obj.Type?.LoopUi(isEditor);
        
        foreach (var child in obj.Children)
            LoopUiObj(child, isEditor, index + 1);
    }

    public static void Loop3DEditor(Viewport viewport) {

        if (ActiveLevel == null) return;
        
        Loop3DEditorObj(ActiveLevel.Root, viewport);
    }
    
    private static void Loop3DEditorObj(Obj obj, Viewport viewport, int index = 0) {
        
        obj.Type?.Loop3DEditor(viewport);
        
        foreach (var child in obj.Children)
            Loop3DEditorObj(child, viewport, index + 1);
    }
    
    public static void LoopUiEditor(Viewport viewport) {

        if (ActiveLevel == null) return;
        
        LoopUiEditorObj(ActiveLevel.Root, viewport);
    }
    
    private static void LoopUiEditorObj(Obj obj, Viewport viewport, int index = 0) {
        
        obj.Type?.LoopUiEditor(viewport);
        
        foreach (var child in obj.Children)
            LoopUiEditorObj(child, viewport, index + 1);
    }

    public static void Quit() {
        
        Shaders.Quit();
        Fonts.UnloadRlFonts();

        if (ActiveLevel == null) return;
        
        QuitObj(ActiveLevel.Root);
    }

    private static void QuitObj(Obj obj, int index = 0) {
        
        obj.Type?.Quit();
        
        foreach (var child in obj.Children)
             QuitObj(child, index + 1);
    }
}