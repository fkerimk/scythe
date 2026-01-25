using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Core {

    public static readonly List<Level> OpenLevels = [];
    public static int ActiveLevelIndex = -1;
    public static Level? ActiveLevel => ActiveLevelIndex >= 0 && ActiveLevelIndex < OpenLevels.Count ? OpenLevels[ActiveLevelIndex] : null;
    public static Camera3D? ActiveCamera;
    public static bool ShouldFocusActiveLevel;
    public static bool IsPreviewRender;

    public static readonly RenderSettings RenderSettings = new();

    private static readonly List<Light> Lights = [];
    private static readonly List<TransparentDrawCall> TransparentRenderQueue = [];

    private static RenderTexture2D _shadowMap;
    private const int ShadowMapResolution = 4096;

    private static Raylib_cs.Model _skyboxModel;
    private static Texture2D _skyboxTexture;
    
    public static unsafe void Init() {
        
        // Physics
        Physics.Init();

        // Fonts
        Fonts.Init();

        // Assets
        AssetManager.Init();
        
        // Setup Global PBR Uniforms
        var pbr = AssetManager.Get<ShaderAsset>("pbr");
        
        if (pbr != null) {
            
            SetShaderValue(pbr.Shader, pbr.GetLoc("use_tex_albedo"), CommandLine.Editor ? Config.Editor.PbrAlbedo : Config.Runtime.PbrAlbedo, ShaderUniformDataType.Int);
            SetShaderValue(pbr.Shader, pbr.GetLoc("use_tex_normal"), CommandLine.Editor ? Config.Editor.PbrNormal : Config.Runtime.PbrNormal, ShaderUniformDataType.Int);
            SetShaderValue(pbr.Shader, pbr.GetLoc("use_tex_mra"), CommandLine.Editor ? Config.Editor.PbrMra : Config.Runtime.PbrMra, ShaderUniformDataType.Int);
            SetShaderValue(pbr.Shader, pbr.GetLoc("use_tex_emissive"), CommandLine.Editor ? Config.Editor.PbrEmissive : Config.Runtime.PbrEmissive, ShaderUniformDataType.Int);
        }

        // Level & camera
        if (!CommandLine.Editor) OpenLevel("Main");
        
        _shadowMap = LoadShadowmapRenderTexture(ShadowMapResolution, ShadowMapResolution);
        
        // Skybox
        var cube = GenMeshCube(1.0f, 1.0f, 1.0f);
        _skyboxModel = LoadModelFromMesh(cube);
        
        var skybox = AssetManager.Get<ShaderAsset>("skybox");
        if (skybox != null) _skyboxModel.Materials[0].Shader = skybox.Shader;
        
        var skyTex = AssetManager.Get<TextureAsset>("Skybox");

        if (skyTex == null) return;
        
        var image = LoadImage(skyTex.File);
        _skyboxTexture = LoadTextureCubemap(image, CubemapLayout.AutoDetect);
        UnloadImage(image);
        _skyboxModel.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = _skyboxTexture;
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
            
            if (
                PathUtil.BestPath($"Levels/{name}.level.json", out var foundPath) ||
                PathUtil.BestPath($"{name}.level.json", out foundPath) ||
                PathUtil.BestPath($"Levels/{name}.json", out foundPath) || 
                PathUtil.BestPath($"{name}.json", out foundPath)
                
            ) level = new Level(name, foundPath);
            
            else {
                
                TraceLog(TraceLogLevel.Error, $"CORE: Could not find level {name}");
                return;
            }
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
        
        if (!CommandLine.Editor) {
            
            if (ActiveLevel.Root.Children.TryGetValue("Camera", out var cameraObj) && cameraObj.Components.TryGetValue("Camera", out var camComp))
                 ActiveCamera = (camComp as Camera)?.Cam;
            else ActiveCamera = FindFirstCamera(ActiveLevel.Root);
        }
        
        else ActiveCamera = new Camera3D();

        if (!CommandLine.Editor) return;
        
        if (ActiveLevel.EditorCamera != null) {
                
            FreeCam.Pos = ActiveLevel.EditorCamera.Position;
            FreeCam.Rot = ActiveLevel.EditorCamera.Rotation;
        }
            
        else FreeCam.SetFromTarget(ActiveCamera);

        return;

        static Camera3D? FindFirstCamera(Obj obj) {
            
            if (obj.Components.Values.FirstOrDefault(c => c is Camera) is Camera found)
                return found.Cam;

            return obj.Children.Values.Select(FindFirstCamera).OfType<Camera3D>().FirstOrDefault();
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

            foreach (var child in obj.Children.Values.ToArray())
                LoadObj(child);
        }
    }

    public static void Logic() {
        
        AssetManager.Update();
        
        if (ActiveLevel == null) return;
        
        Lights.Clear();
        TransparentRenderQueue.Clear();
        
        if (!CommandLine.Editor) Physics.Update();
        
        // Behavior & Logic (Scripts, Tweens, Physics Sync) This pass determines WHERE objects want to be in this frame.
        RunLogic(ActiveLevel.Root);

        // Hierarchy Sync & Visuals (World Matrices + Cartoon Bounce) his pass settles the physical truth and prepares the visual state for rendering.
        SyncHierarchy(ActiveLevel.Root);
        
        // Update global pbr parameters after everything is settled
        var pbr = AssetManager.Get<ShaderAsset>("pbr");
        if (pbr != null)
            SetShaderValue(pbr.Shader, pbr.GetLoc("view_pos"), ActiveCamera?.Position ?? Vector3.Zero, ShaderUniformDataType.Vec3);
        
        return;

        void RunLogic(Obj obj) {
            
            // Priority: Rigidbodies must sync physics to transform before scripts run
            if (obj.Components.TryGetValue("Rigidbody", out var rb) && rb.IsLoaded)
                rb.Logic();

            foreach (var component in obj.Components.Values) {
                
                if (component is Rigidbody) continue;
                if (component.IsLoaded) component.Logic();
            }

            foreach (var child in obj.Children.Values.ToArray()) RunLogic(child);
        }

        void SyncHierarchy(Obj obj) {
            
            // Physical World Sync (Top-Down)
            if (obj.Parent != null) {
                
                obj.WorldMatrix = obj.Parent.WorldMatrix * obj.Matrix;
                obj.WorldRotMatrix = obj.Parent.WorldRotMatrix * obj.RotMatrix;
                
            } else {
                
                obj.WorldMatrix = obj.Matrix;
                obj.WorldRotMatrix = obj.RotMatrix;
            }

            // Visual State Sync
            if (!CommandLine.Editor) {
                
                // Runtime: Instant sync for zero lag (Physics etc.)
                obj.VisualWorldMatrix = obj.WorldMatrix;
                
            } else {
                
                // Handle Cartoon Bounce Start with a clean inherited baseline
                obj.VisualWorldMatrix = obj.Parent != null
                    ? obj.Parent.VisualWorldMatrix * obj.Matrix
                    : obj.WorldMatrix;

                // If this is the selected object or undergoing local bounce, UpdateCartoon will override it
                obj.Transform.UpdateCartoon();
            }

            // 3. Collection & Preparation
            foreach (var component in obj.Components.Values) {
                
                if (!component.IsLoaded) continue;

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

            foreach (var child in obj.Children.Values.ToArray()) SyncHierarchy(child);
        }
    }

    public static void ShadowPass() {
        
        if (ActiveLevel == null) return;
        
        var pbr = AssetManager.Get<ShaderAsset>("pbr");
        if (pbr == null) return;

        SetShaderValue(pbr.Shader, pbr.GetLoc("light_count"), Lights.Count, ShaderUniformDataType.Int);
            
        var shadowLight = Lights.FirstOrDefault(l => l is { Enabled: true, Shadows: true });

        if (shadowLight != null) {
            
            var shadowLightIndex = Lights.IndexOf(shadowLight);

            var pos = shadowLight.Obj.Transform.WorldPos;
            var fwd = shadowLight.Obj.Fwd;

            var lightCamera = new Raylib_cs.Camera3D {
                
                Position = shadowLight.Type == 0 ? pos - fwd * 500.0f : pos,
                
                Target = shadowLight.Type switch {
                    
                    0 => pos,
                    1 => pos + Vector3.UnitY * -1,
                    _ => pos + fwd
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
            var depth = AssetManager.Get<ShaderAsset>("depth");
            
            if (depth != null) {
                
                BeginShaderMode(depth.Shader);
                RenderHierarchy(ActiveLevel.Root, false, true);
                EndShaderMode();
            }

            EndMode3D();
            EndTextureMode();
            
            SetShaderValueMatrix(pbr.Shader, pbr.GetLoc("lightVP"), lightVp);
            SetShaderValue(pbr.Shader, pbr.GetLoc("shadow_light_index"), shadowLightIndex, ShaderUniformDataType.Int);
            SetShaderValue(pbr.Shader, pbr.GetLoc("shadow_strength"), shadowLight.ShadowStrength, ShaderUniformDataType.Float);
            SetShaderValue(pbr.Shader, pbr.GetLoc("shadow_map_resolution"), ShadowMapResolution, ShaderUniformDataType.Int);

            const int shadowMapSlot = 10;
            Rlgl.ActiveTextureSlot(shadowMapSlot);
            Rlgl.EnableTexture(_shadowMap.Depth.Id);
            SetShaderValue(pbr.Shader, pbr.GetLoc("shadowMap"), shadowMapSlot, ShaderUniformDataType.Int);
            Rlgl.ActiveTextureSlot(0);
            
        }
        
        else SetShaderValue(pbr.Shader, pbr.GetLoc("shadow_light_index"), -1, ShaderUniformDataType.Int);

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
                
                foreach (var call in TransparentRenderQueue)
                    call.Model.Draw(0.0f);
                
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
        
        foreach (var child in obj.Children.Values.ToArray()) RenderHierarchy(child, is2D, isShadowPass);
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
        
        Fonts.UnloadRlFonts();
        AssetManager.UnloadAll();

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
            
            foreach (var child in obj.Children.Values.ToArray()) QuitObj(child);
        }
    }
}