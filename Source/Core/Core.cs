using System.Numerics;
using Raylib_cs;

internal static class Core {

    public static Level? ActiveLevel;
    public static Camera3D? ActiveCamera;
    
    public static readonly List<Light> Lights = [];
    
    public struct TransparentDrawCall {
        public Model Model;
        public float Distance;
    }
    
    public static RenderTexture2D ShadowMap;
    public const int ShadowMapResolution = 4096;
    
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
        
        ShadowMap = LoadShadowmapRenderTexture(ShadowMapResolution, ShadowMapResolution);
    }

    private static RenderTexture2D LoadShadowmapRenderTexture(int width, int height) {
        
        var target = new RenderTexture2D {
            Id = Rlgl.LoadFramebuffer()
        };
        target.Texture.Width = width;
        target.Texture.Height = height;

        if (target.Id > 0) {
            
            Rlgl.EnableFramebuffer(target.Id);
            target.Depth.Id = Rlgl.LoadTextureDepth(width, height, false);
            target.Depth.Width = width;
            target.Depth.Height = height;
            target.Depth.Format = PixelFormat.UncompressedGrayscale; // 19? In C# it might be different. 
            target.Depth.Mipmaps = 1;

            Rlgl.FramebufferAttach(target.Id, target.Depth.Id, FramebufferAttachType.Depth, FramebufferAttachTextureType.Texture2D, 0);

            if (Rlgl.FramebufferComplete(target.Id)) Raylib.TraceLog(TraceLogLevel.Info, "FBO: Shadowmap created successfully");
            
            Raylib.SetTextureFilter(target.Depth, TextureFilter.Bilinear);
            Raylib.SetTextureWrap(target.Depth, TextureWrap.Clamp);

            Rlgl.DisableFramebuffer();
        }
        
        return target;
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
        
        // Shadow Pass
        var shadowLight = Lights.FirstOrDefault(l => l.Enabled && l.Shadows);
        var shadowLightIndex = -1;
        
        if (shadowLight != null) {
            
            shadowLightIndex = Lights.IndexOf(shadowLight);

            var lightCamera = new Camera3D {
                Position = shadowLight.Type == 0 ? shadowLight.Obj.Pos - shadowLight.Obj.Fwd * 500.0f : shadowLight.Obj.Pos,
                Target = shadowLight.Type == 0 ? shadowLight.Obj.Pos : 
                         (shadowLight.Type == 1 ? shadowLight.Obj.Pos + Vector3.UnitY * -1 : shadowLight.Obj.Pos + shadowLight.Obj.Fwd),
                Up = shadowLight.Type == 1 ? Vector3.UnitX : Vector3.UnitY,
                FovY = shadowLight.Type == 0 ? shadowLight.Range * 2.0f : (shadowLight.Type == 2 ? 90.0f : 160.0f),
                Projection = shadowLight.Type == 0 ? CameraProjection.Orthographic : CameraProjection.Perspective
            };
            
            // Note: For Orthographic, FovY is the total size. 
            // We use Range * 2 to define the box size around the light target.

            Raylib.BeginTextureMode(ShadowMap);
            Raylib.ClearBackground(Color.White);
            Raylib.BeginMode3D(lightCamera.Raylib);
            
            var lightView = Rlgl.GetMatrixModelview();
            var lightProj = Rlgl.GetMatrixProjection();
            var lightVP = Raymath.MatrixMultiply(lightView, lightProj);

            // Draw all shadow casters
            LoopDraw(ActiveLevel.Root, true);

            Raylib.EndMode3D();
            Raylib.EndTextureMode();
            
            Raylib.SetShaderValueMatrix(Shaders.Pbr, Shaders.PbrLightVP, lightVP);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowLightIndex, shadowLightIndex, ShaderUniformDataType.Int);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowStrength, shadowLight.ShadowStrength, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowMapResolution, ShadowMapResolution, ShaderUniformDataType.Int);

            // Bind shadow map
            const int shadowMapSlot = 10;
            Rlgl.ActiveTextureSlot(shadowMapSlot);
            Rlgl.EnableTexture(ShadowMap.Depth.Id);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowMap, shadowMapSlot, ShaderUniformDataType.Int);
        }
        else {
            
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowLightIndex, -1, ShaderUniformDataType.Int);
        }

        for (var i = 0; i < Lights.Count; i++) Lights[i].Update(i);

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
        
        Raylib.UnloadRenderTexture(ShadowMap);
        QuitObj(ActiveLevel.Root);
        
        return;
        
        void QuitObj(Obj obj) {

            foreach (var component in obj.Components.Values) component.Quit();
            foreach (var child in obj.Children) QuitObj(child.Value);
        }
    }

    private static void LoopDraw(Obj obj, bool isShadowPass) {
        
        foreach (var component in obj.Components.Values) {
            
            if (component is Model model) {
                if (isShadowPass) {
                    if (model.CastShadows) {
                        // Use depth shader
                        Raylib.BeginShaderMode(Shaders.Depth);
                        Raylib.DrawModel(model.RlModel, Vector3.Zero, 1, Color.White);
                        Raylib.EndShaderMode();
                    }
                }
            }
        }

        foreach (var child in obj.Children)
            LoopDraw(child.Value, isShadowPass);
    }
}