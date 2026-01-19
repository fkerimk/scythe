using System.Numerics;
using Raylib_cs;

internal static class Core {

    public static Level? ActiveLevel;
    public static Camera3D? ActiveCamera;

    public static readonly RenderSettings RenderSettings = new(false);

    private static readonly List<Light> Lights = [];
    private static readonly List<TransparentDrawCall> TransparentRenderQueue = [];

    private static RenderTexture2D _shadowMap;
    private const int ShadowMapResolution = 4096;

    private static Raylib_cs.Model _skyboxModel;
    private static Texture2D _skyboxTexture;
    
    public static bool IsRendering;

    public static unsafe void Init() {

        // Shaders
        Shaders.Init();

        // Physics
        Physics.Init();

        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_albedo"  ), CommandLine.Editor ? Config.Editor.PbrAlbedo   : Config.Runtime.PbrAlbedo , ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_normal"  ), CommandLine.Editor ? Config.Editor.PbrNormal   : Config.Runtime.PbrNormal, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_mra"     ), CommandLine.Editor ? Config.Editor.PbrMra      : Config.Runtime.PbrMra, ShaderUniformDataType.Int);
        Raylib.SetShaderValue(Shaders.Pbr, Raylib.GetShaderLocation(Shaders.Pbr, "use_tex_emissive"), CommandLine.Editor ? Config.Editor.PbrEmissive : Config.Runtime.PbrEmissive, ShaderUniformDataType.Int);

        // Fonts
        Fonts.Init();
        
        // Level & camera
        ActiveLevel = new Level("Main");
        ActiveCamera = CommandLine.Editor ? new Camera3D() : (ActiveLevel.Root.Children["Camera"].Components["Camera"] as Camera)?.Cam;
        
        _shadowMap = LoadShadowmapRenderTexture(ShadowMapResolution, ShadowMapResolution);
        
        // Skybox
        var cube = Raylib.GenMeshCube(1.0f, 1.0f, 1.0f);
        _skyboxModel = Raylib.LoadModelFromMesh(cube);
        _skyboxModel.Materials[0].Shader = Shaders.Skybox;
        
        if (PathUtil.BestPath("Models/Skybox.png", out var skyboxPath)) {
            
            var image = Raylib.LoadImage(skyboxPath);
            _skyboxTexture = Raylib.LoadTextureCubemap(image, CubemapLayout.AutoDetect);
            Raylib.UnloadImage(image);
            _skyboxModel.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = _skyboxTexture;
        }
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
        
        void LoadObj(Obj obj) {

            foreach (var component in obj.Components.Values) {
            
                if (!component.IsLoaded && component.Load())
                    component.IsLoaded = true;
            }

            foreach (var child in obj.Children)
                LoadObj(child.Value);
        }
    }

    public static unsafe void Logic() {
        
        if (ActiveLevel == null) return;
        
        Lights.Clear();
        TransparentRenderQueue.Clear();
        
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.Pbr.Locs[(int)ShaderLocationIndex.VectorView], ActiveCamera?.Position ?? Vector3.Zero, ShaderUniformDataType.Vec3);
        
        if (!CommandLine.Editor) Physics.Update();
        
        UpdateObj(ActiveLevel.Root);

        void UpdateObj(Obj obj) {
            
            if (obj.Parent != null) {
                
                obj.WorldMatrix = obj.Parent.WorldMatrix * obj.Matrix;
                obj.WorldRotMatrix = obj.Parent.WorldRotMatrix * obj.RotMatrix;
                
            } else {
                
                obj.WorldMatrix = obj.Matrix;
                obj.WorldRotMatrix = obj.RotMatrix;
            }
            
            // Logic and component updates
            obj.Transform.Loop(false);
            
            foreach (var component in obj.Components.Values) {
                
                component.Loop(false);
                
                switch (component) {
                    
                    case Light light: Lights.Add(light); break;
                    
                    case Model { IsTransparent: true } model: {
                        
                        var worldPos = new Vector3(obj.WorldMatrix.M41, obj.WorldMatrix.M42, obj.WorldMatrix.M43);
                        var distance = Vector3.Distance(ActiveCamera?.Position ?? Vector3.Zero, worldPos);
                        TransparentRenderQueue.Add(new TransparentDrawCall { Model = model, Distance = distance });
                        break;
                    }
                }
            }

            foreach (var child in obj.Children) UpdateObj(child.Value);
        }
    }

    public static void ShadowPass() {
        
        if (ActiveLevel == null) return;

        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrLightCount, Lights.Count, ShaderUniformDataType.Int);
            
        var shadowLight = Lights.FirstOrDefault(l => l is { Enabled: true, Shadows: true });

        if (shadowLight != null) {
            
            var shadowLightIndex = Lights.IndexOf(shadowLight);

            var lightCamera = new Raylib_cs.Camera3D {
                
                Position = shadowLight.Type == 0 ? shadowLight.Obj.Pos - shadowLight.Obj.Fwd * 500.0f : shadowLight.Obj.Pos,
                
                Target = shadowLight.Type switch {
                    
                    0 => shadowLight.Obj.Pos,
                    1 => shadowLight.Obj.Pos + Vector3.UnitY * -1,
                    _ => shadowLight.Obj.Pos + shadowLight.Obj.Fwd
                },
                
                Up = shadowLight.Type == 1 ? Vector3.UnitX : Vector3.UnitY,
                FovY = (shadowLight.Type == 0 ? shadowLight.Range * 2.0f : (shadowLight.Type == 2 ? 90.0f : 160.0f)) * RenderSettings.ShadowFovScale,
                Projection = shadowLight.Type == 0 ? CameraProjection.Orthographic : CameraProjection.Perspective
            };

            Raylib.BeginTextureMode(_shadowMap);
            Raylib.ClearBackground(Color.White);
            Raylib.BeginMode3D(lightCamera);
            
            var lightView = Rlgl.GetMatrixModelview();
            var lightProj = Rlgl.GetMatrixProjection();
            var lightVp = Raymath.MatrixMultiply(lightView, lightProj);

            // Draw objects for shadow depth
            Raylib.BeginShaderMode(Shaders.Depth);
            RenderHierarchy(ActiveLevel.Root, false, true);
            Raylib.EndShaderMode();

            Raylib.EndMode3D();
            Raylib.EndTextureMode();
            
            Raylib.SetShaderValueMatrix(Shaders.Pbr, Shaders.PbrLightVp, lightVp);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowLightIndex, shadowLightIndex, ShaderUniformDataType.Int);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowStrength, shadowLight.ShadowStrength, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowMapResolution, ShadowMapResolution, ShaderUniformDataType.Int);

            const int shadowMapSlot = 10;
            Rlgl.ActiveTextureSlot(shadowMapSlot);
            Rlgl.EnableTexture(_shadowMap.Depth.Id);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowMap, shadowMapSlot, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(0);
            
        }
        
        else Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrShadowLightIndex, -1, ShaderUniformDataType.Int);

        for (var i = 0; i < Lights.Count; i++) Lights[i].Update(i);
    }

    public static void Render(bool is2D) {
        
        if (ActiveLevel == null) return;
        
        IsRendering = true;

        if (!is2D) {
            
            // Skybox
            Rlgl.DisableBackfaceCulling();
            Rlgl.DisableDepthMask();
            Raylib.DrawModel(_skyboxModel, Vector3.Zero, 1.0f, Color.White);
            Rlgl.EnableBackfaceCulling();
            Rlgl.EnableDepthMask();
        }

        RenderHierarchy(ActiveLevel.Root, is2D, false);

        if (!is2D) {
            
            if (TransparentRenderQueue.Count > 0) {
                
                TransparentRenderQueue.Sort((a, b) => b.Distance.CompareTo(a.Distance));
                
                Rlgl.DisableDepthMask();
                Raylib.BeginBlendMode(BlendMode.Alpha);
                
                foreach (var call in TransparentRenderQueue) {
                    
                    Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrAlphaCutoff, 0.0f, ShaderUniformDataType.Float);
                    call.Model.Draw();
                }
                
                Raylib.EndBlendMode();
                Rlgl.EnableDepthMask();
            }
        }
        
        IsRendering = false;
    }

    private static void RenderHierarchy(Obj obj, bool is2D, bool isShadowPass) {
        
        // Ensure components drawing. Transform is updated in Logic
        
        if (isShadowPass) {
            
            foreach (var component in obj.Components.Values) {
                
                if (component is Model { CastShadows: true } m)
                    m.DrawShadow();
            }
            
        } else {
            
            obj.Transform.Loop(is2D);
            
            foreach (var component in obj.Components.Values) {
                
                component.Loop(is2D);
            }
        }
        
        foreach (var child in obj.Children) RenderHierarchy(child.Value, is2D, isShadowPass);
    }

    public static void Loop(bool is2D) {
        
        if (is2D) Render(true);
        
        else {
            
            Logic();
            ShadowPass();
            
            Render(false);
        }
    }

    public static void Quit() {
        
        Shaders.Quit();
        Fonts.UnloadRlFonts();

        if (ActiveLevel == null) return;
        
        Raylib.UnloadRenderTexture(_shadowMap);
        Raylib.UnloadModel(_skyboxModel);
        Raylib.UnloadTexture(_skyboxTexture);
        
        QuitObj(ActiveLevel.Root);
        
        return;
        
        void QuitObj(Obj obj) {

            foreach (var component in obj.Components.Values) component.Quit();
            foreach (var child in obj.Children) QuitObj(child.Value);
        }
    }
}