using System.Numerics;
using Raylib_cs;

internal static class Core {

    public static Level? ActiveLevel;
    public static Camera3D? ActiveCamera;
    
    public static readonly Dictionary<int, Light> Lights = [];
    
    public struct TransparentDrawCall {
        public Model Model;
        public float Distance;
    }
    
    public static readonly List<TransparentDrawCall> TransparentRenderQueue = [];

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
        ActiveCamera = CommandLine.Editor ? new Camera3D() : (ActiveLevel.Root.Children["Camera"].Components["Camera"] as Camera)?.Cam;
    }

    public static void Load() {
        
        if (ActiveLevel == null) return;
        
        LoadObj(ActiveLevel.Root);
        
        return;
        
        void LoadObj(Obj obj) {

            foreach (var component in obj.Components.Values) {
            
                if (!component.IsLoaded && component.Load())
                    component.IsLoaded = true;
            }

            foreach (var child in obj.Children)
                LoadObj(child.Value);
        }
    }
    
    public static unsafe void Loop(bool is2D) {

        if (ActiveLevel == null) return;

        Lights.Clear();
        TransparentRenderQueue.Clear();
        
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.Pbr.Locs[(int)ShaderLocationIndex.VectorView], ActiveCamera?.Position ?? Vector3.Zero, ShaderUniformDataType.Vec3);
        
        LoopObj(ActiveLevel.Root);
        
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrLightCount, Lights.Count, ShaderUniformDataType.Int);
        
        foreach (var light in Lights.Values) light.Update();

        if (!is2D && TransparentRenderQueue.Count > 0) {
            
            TransparentRenderQueue.Sort((a, b) => b.Distance.CompareTo(a.Distance));
            
            foreach (var call in TransparentRenderQueue) {
                
                call.Model.DrawTransparent();
            }
        }
        
        return;
        
        void LoopObj(Obj obj) {
        
            if (obj.Parent != null) {
            
                obj.WorldMatrix = obj.Parent.WorldMatrix * obj.Matrix;
                obj.WorldRotMatrix = obj.Parent.WorldRotMatrix * obj.RotMatrix;
            }
            else {

                obj.WorldMatrix = obj.Matrix;
                obj.WorldRotMatrix = obj.RotMatrix;
            }

            obj.Transform.Loop(is2D);
            
            foreach (var component in obj.Components.Values)
                component.Loop(is2D);
        
            foreach (var child in obj.Children)
                LoopObj(child.Value);
        }
    }

    public static void Quit() {
        
        Shaders.Quit();
        Fonts.UnloadRlFonts();

        if (ActiveLevel == null) return;
        
        QuitObj(ActiveLevel.Root);
        
        return;
        
        void QuitObj(Obj obj) {

            foreach (var component in obj.Components.Values) component.Quit();
            foreach (var child in obj.Children) QuitObj(child.Value);
        }
    }
}