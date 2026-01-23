using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Core {

    public static readonly List<Level> OpenLevels = [];
    public static int ActiveLevelIndex = -1;
    public static Level? ActiveLevel => ActiveLevelIndex >= 0 && ActiveLevelIndex < OpenLevels.Count ? OpenLevels[ActiveLevelIndex] : null;
    public static Camera3D? ActiveCamera;
    public static bool ShouldFocusActiveLevel;

    public static readonly RenderSettings RenderSettings = new(false);

    private static readonly List<Light> Lights = [];
    private static readonly List<TransparentDrawCall> TransparentRenderQueue = [];

    private static RenderTexture2D _shadowMap;
    private const int ShadowMapResolution = 4096;

    private static Raylib_cs.Model _skyboxModel;
    private static Texture2D _skyboxTexture;
    
    public static unsafe void Init() {

        // Shaders
        Shaders.Init();

        // Physics
        Physics.Init();

        SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "use_tex_albedo"  ), CommandLine.Editor ? Config.Editor.PbrAlbedo   : Config.Runtime.PbrAlbedo , ShaderUniformDataType.Int);
        SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "use_tex_normal"  ), CommandLine.Editor ? Config.Editor.PbrNormal   : Config.Runtime.PbrNormal, ShaderUniformDataType.Int);
        SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "use_tex_mra"     ), CommandLine.Editor ? Config.Editor.PbrMra      : Config.Runtime.PbrMra, ShaderUniformDataType.Int);
        SetShaderValue(Shaders.Pbr, GetShaderLocation(Shaders.Pbr, "use_tex_emissive"), CommandLine.Editor ? Config.Editor.PbrEmissive : Config.Runtime.PbrEmissive, ShaderUniformDataType.Int);

        // Fonts
        Fonts.Init();
        
        // Level & camera
        if (!CommandLine.Editor) OpenLevel("Main");
        
        _shadowMap = LoadShadowmapRenderTexture(ShadowMapResolution, ShadowMapResolution);
        
        // Skybox
        var cube = GenMeshCube(1.0f, 1.0f, 1.0f);
        _skyboxModel = LoadModelFromMesh(cube);
        _skyboxModel.Materials[0].Shader = Shaders.Skybox;
        
        if (PathUtil.BestPath("Models/Skybox.png", out var skyboxPath)) {
            
            var image = LoadImage(skyboxPath);
            _skyboxTexture = LoadTextureCubemap(image, CubemapLayout.AutoDetect);
            UnloadImage(image);
            _skyboxModel.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = _skyboxTexture;
        }
    }

    private static RenderTexture2D LoadShadowmapRenderTexture(int width, int height) {
        
        var target = new RenderTexture2D { Id = Rlgl.LoadFramebuffer() };
        
        target.Texture.Width = width;
        target.Texture.Height = height;

        if (target.Id <= 0) return target;
        
        Rlgl.EnableFramebuffer(target.Id);
        target.Depth.Id = Rlgl.LoadTextureDepth(width, height, false);
        target.Depth.Width = width;
        target.Depth.Height = height;
        target.Depth.Format = PixelFormat.UncompressedGrayscale;
        target.Depth.Mipmaps = 1;

        Rlgl.FramebufferAttach(target.Id, target.Depth.Id, FramebufferAttachType.Depth, FramebufferAttachTextureType.Texture2D, 0);

        if (Rlgl.FramebufferComplete(target.Id)) TraceLog(TraceLogLevel.Info, "FBO: Shadowmap created successfully");
            
        SetTextureFilter(target.Depth, TextureFilter.Bilinear);
        SetTextureWrap(target.Depth, TextureWrap.Clamp);

        Rlgl.DisableFramebuffer();

        return target;
    }

    public static void OpenLevel(string name, string? path = null) {
        
        Level? level;
        
        if (path == null) {
            
            if (PathUtil.BestPath($"Levels/{name}.json", out var foundPath))
                 level = new Level(name, foundPath);
            else return;
        }
        else level = new Level(name, path);

        var existingIndex = OpenLevels.FindIndex(l => Path.GetFullPath(l.JsonPath).Equals(Path.GetFullPath(level.JsonPath), StringComparison.OrdinalIgnoreCase));
        
        if (existingIndex != -1) {
            
            SetActiveLevel(existingIndex);
            ShouldFocusActiveLevel = true;
            return;
        }

        OpenLevels.Add(level);
        SetActiveLevel(OpenLevels.Count - 1);
        ShouldFocusActiveLevel = true;
        
        Load();
    }

    public static void SetActiveLevel(int index) {
        
        if (index < 0 || index >= OpenLevels.Count) return;
        
        ActiveLevelIndex = index;
        
        if (ActiveLevel == null) return;
        
        History.Clear(); // Clear history when switching levels to avoid cross-level undo
        ActiveLevel.Root.Transform.UpdateTransform();
        ActiveCamera = CommandLine.Editor ? new Camera3D() : (ActiveLevel.Root.Children["Camera"].Components["Camera"] as Camera)?.Cam;
        
        if (CommandLine.Editor) {
            
            if (ActiveLevel.EditorCamera != null) {
                FreeCam.Pos = ActiveLevel.EditorCamera.Position;
                FreeCam.Rot = ActiveLevel.EditorCamera.Rotation;
            } else {
                FreeCam.SetFromTarget(ActiveCamera);
            }
        }
    }

    public static void CloseLevel(int index) {
        
        if (index < 0 || index >= OpenLevels.Count) return;
        
        OpenLevels.RemoveAt(index);
        
        if (ActiveLevelIndex >= OpenLevels.Count)
            ActiveLevelIndex = OpenLevels.Count - 1;
        
        if (ActiveLevelIndex >= 0) SetActiveLevel(ActiveLevelIndex);
        else ActiveCamera = null;
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

    public static unsafe void Logic() {
        
        if (ActiveLevel == null) return;
        
        Lights.Clear();
        TransparentRenderQueue.Clear();
        
        SetShaderValue(Shaders.Pbr, Shaders.Pbr.Locs[(int)ShaderLocationIndex.VectorView], ActiveCamera?.Position ?? Vector3.Zero, ShaderUniformDataType.Vec3);
        
        if (!CommandLine.Editor) Physics.Update();
        
        UpdateObj(ActiveLevel.Root);
        
        return;

        void UpdateObj(Obj obj) {
            
            if (obj.Parent != null) {
                
                obj.WorldMatrix = obj.Parent.WorldMatrix * obj.Matrix;
                obj.WorldRotMatrix = obj.Parent.WorldRotMatrix * obj.RotMatrix;
                
                // Inherit parent's visual transform so children follow the bounce
                obj.VisualWorldMatrix = obj.Parent.VisualWorldMatrix * obj.Matrix;
                
            } else {
                
                obj.WorldMatrix = obj.Matrix;
                obj.WorldRotMatrix = obj.RotMatrix;
                obj.VisualWorldMatrix = obj.WorldMatrix;
            }
            
            // Logic and component updates
            obj.Transform.Logic();
            
            foreach (var component in obj.Components.Values) {
                
                if (component.IsLoaded)
                    component.Logic();
                
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

        SetShaderValue(Shaders.Pbr, Shaders.PbrLightCount, Lights.Count, ShaderUniformDataType.Int);
            
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

            BeginTextureMode(_shadowMap);
            ClearBackground(Color.White);
            BeginMode3D(lightCamera);
            
            var lightView = Rlgl.GetMatrixModelview();
            var lightProj = Rlgl.GetMatrixProjection();
            var lightVp = Raymath.MatrixMultiply(lightView, lightProj);

            // Draw objects for shadow depth
            BeginShaderMode(Shaders.Depth);
            RenderHierarchy(ActiveLevel.Root, false, true);
            EndShaderMode();

            EndMode3D();
            EndTextureMode();
            
            SetShaderValueMatrix(Shaders.Pbr, Shaders.PbrLightVp, lightVp);
            SetShaderValue(Shaders.Pbr, Shaders.PbrShadowLightIndex, shadowLightIndex, ShaderUniformDataType.Int);
            SetShaderValue(Shaders.Pbr, Shaders.PbrShadowStrength, shadowLight.ShadowStrength, ShaderUniformDataType.Float);
            SetShaderValue(Shaders.Pbr, Shaders.PbrShadowMapResolution, ShadowMapResolution, ShaderUniformDataType.Int);

            const int shadowMapSlot = 10;
            Rlgl.ActiveTextureSlot(shadowMapSlot);
            Rlgl.EnableTexture(_shadowMap.Depth.Id);
            SetShaderValue(Shaders.Pbr, Shaders.PbrShadowMap, shadowMapSlot, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(0);
            
        }
        
        else SetShaderValue(Shaders.Pbr, Shaders.PbrShadowLightIndex, -1, ShaderUniformDataType.Int);

        for (var i = 0; i < Lights.Count; i++) Lights[i].Update(i);
    }

    public static void Render(bool is2D) {
        
        if (ActiveLevel == null) return;
        
        if (!is2D) {
            
            // Skybox
            Rlgl.DisableBackfaceCulling();
            Rlgl.DisableDepthMask();
            DrawModel(_skyboxModel, Vector3.Zero, 1.0f, Color.White);
            Rlgl.EnableBackfaceCulling();
            Rlgl.EnableDepthMask();
        }

        RenderHierarchy(ActiveLevel.Root, is2D, false);

        if (!is2D) {
            
            if (TransparentRenderQueue.Count > 0) {
                
                TransparentRenderQueue.Sort((a, b) => b.Distance.CompareTo(a.Distance));
                
                Rlgl.DisableDepthMask();
                BeginBlendMode(BlendMode.Alpha);
                
                foreach (var call in TransparentRenderQueue) {
                    
                    SetShaderValue(Shaders.Pbr, Shaders.PbrAlphaCutoff, 0.0f, ShaderUniformDataType.Float);
                    call.Model.Draw();
                }
                
                EndBlendMode();
                Rlgl.EnableDepthMask();
            }
        }
    }

    private static void RenderHierarchy(Obj obj, bool is2D, bool isShadowPass) {
        
        // Ensure components drawing. Transform is updated in Logic
        
        if (isShadowPass) {
            
            foreach (var component in obj.Components.Values) {
                
                if (component is Model { CastShadows: true, IsLoaded: true } m)
                    m.DrawShadow();
            }
            
        } else {
            
            if (is2D)
                 obj.Transform.Render2D();
            else obj.Transform.Render3D();
            
            foreach (var component in obj.Components.Values) {
                
                if (!component.IsLoaded) continue;
                
                if (is2D)
                     component.Render2D();
                else component.Render3D();
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
        
        CloseAudioDevice();
        
        Shaders.Quit();
        Fonts.UnloadRlFonts();

        if (ActiveLevel == null) return;
        
        UnloadRenderTexture(_shadowMap);
        UnloadModel(_skyboxModel);
        UnloadTexture(_skyboxTexture);
        
        QuitObj(ActiveLevel.Root);
        
        return;
        
        void QuitObj(Obj obj) {
            
            obj.Transform.Quit();

            foreach (var component in obj.Components.Values) {
                
                if (component.IsLoaded)
                    component.Unload();
                
                component.Quit();
            }
            
            foreach (var child in obj.Children) QuitObj(child.Value);
        }
    }
}