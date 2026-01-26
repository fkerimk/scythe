using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using static ImGuiNET.ImGui;
using static Raylib_cs.Raylib;
using static Raylib_cs.Rlgl;
using static rlImGui_cs.rlImGui;

internal static unsafe class Editor {

    private static bool     _scheduledQuit;
    private static Camera3D _editorCamera = null!;

    public static ImGuiIOPtr ImGuiIoPtr;

    // ReSharper disable MemberCanBePrivate.Global
    public static EditorRender   EditorRender   = null!;
    public static LevelBrowser   LevelBrowser   = null!;
    public static ProjectBrowser ProjectBrowser = null!;
    public static ObjectBrowser  ObjectBrowser  = null!;
    public static ScriptEditor   ScriptEditor   = null!;
    public static MusicPlayer    MusicPlayer    = null!;
    public static Preview        Preview        = null!;
    public static RuntimeRender  RuntimeRender  = null!;

    // ReSharper restore MemberCanBePrivate.Global

    private static Level? _editorLevelRef;

    public static bool IsScriptEditorFocused => ScriptEditor.IsFocused;

    public static void OpenScript(string path) => ScriptEditor.Open(path);

    public static void OpenLevel(string path) {
        var name = Path.GetFileNameWithoutExtension(path);
        Core.OpenLevel(name, path);
    }

    public static void CreateLevel(string path) {
        var name  = Path.GetFileNameWithoutExtension(path);
        var level = new Level(name, path, false);

        Core.OpenLevels.Add(level);
        Core.SetActiveLevel(Core.OpenLevels.Count - 1);
        level.Save();
        Core.Load();
    }

    public static void Show() {
        Window.Show(flags: [ConfigFlags.Msaa4xHint, ConfigFlags.ResizableWindow], title: $"{Config.Mod.Name} - Editor");

        // Setup ImGui
        Setup(true, true);

        EditorRender = new EditorRender { CustomStyle = new CustomStyle { WindowPadding = new Vector2(0, 0), CellPadding = new Vector2(0, 0), SeparatorTextPadding = new Vector2(0, 0) } };

        LevelBrowser   = new LevelBrowser();
        ProjectBrowser = new ProjectBrowser();
        ObjectBrowser  = new ObjectBrowser();
        ScriptEditor   = new ScriptEditor();
        MusicPlayer    = new MusicPlayer();
        Preview        = new Preview();
        RuntimeRender  = new RuntimeRender();

        var layoutPath = PathUtil.ExeRelative("Layouts/User.ini");

        if (PathUtil.BestPath("Layouts/User.ini", out var existLayoutPath)) layoutPath = existLayoutPath;

        ImGuiIoPtr                        =  GetIO();
        ImGuiIoPtr.ConfigFlags            |= ImGuiConfigFlags.DockingEnable;
        ImGuiIoPtr.NativePtr->IniFilename =  (byte*)Marshal.StringToHGlobalAnsi(layoutPath).ToPointer();

        // Setup core
        Core.Init();
        _editorCamera = new Camera3D();
        FreeCam.SetFromTarget(Core.ActiveCamera);

        EditorRender.Load();

        ViewSettings.Load();
        MusicPlayer.Load();
        ScriptEditor.Load();
        ProjectBrowser.Load();

        Core.Load();

        while (!WindowShouldClose()) {
            Window.UpdateFps();

            if (Core.ActiveLevel == null || Core.ActiveCamera == null) {
                BeginDrawing();
                ClearBackground(Color.Black);
                Begin();
                Style.Push();
                PushFont(Fonts.ImMontserratRegular);
                DockSpaceOverViewport(GetMainViewport().ID);

                MenuBar.Draw();
                EditorRender.Draw();
                ProjectBrowser.Draw();
                Preview.Draw();
                ScriptEditor.Draw();

                PopFont();
                Style.Pop();
                rlImGui.End();
                Notifications.Draw();
                EndDrawing();

                if (_scheduledQuit) break;

                continue;
            }

            // Sync Input State & Focus
            // LuaMouse.InputAllowed = RuntimeRender.IsFocused;

            // Start Drawing & ImGui Frame
            BeginDrawing();

            // --- CORE SIMULATION ---
            // Run logic inside BeginDrawing (for timing) but BEFORE rlImGui (to avoid input/state conflicts)
            Core.ActiveCamera = Core.IsPlaying ? Core.GameCamera : _editorCamera;
            Core.Logic();
            Core.ShadowPass();

            ClearBackground(Color.Black);
            Begin();
            Style.Push();
            PushFont(Fonts.ImMontserratRegular);

            DockSpaceOverViewport(GetMainViewport().ID);
            ImGuiIoPtr.MouseDoubleClickTime = 0.2f;

            // Handle Editor UI Lock when playing with mouse locked
            if (LuaMouse.IsLocked) {
                GetIO().ConfigFlags |= ImGuiConfigFlags.NoMouse;
                GetIO().ConfigFlags |= ImGuiConfigFlags.NoKeyboard;
            } else {
                GetIO().ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
                GetIO().ConfigFlags &= ~ImGuiConfigFlags.NoKeyboard;
            }

            // Reload Viewport Textures if Resized
            if (EditorRender.TexSize != EditorRender.TexTemp) {
                UnloadRenderTexture(EditorRender.Rt);
                EditorRender.Rt = Core.LoadRenderTextureWithDepth((int)EditorRender.TexSize.X, (int)EditorRender.TexSize.Y);

                UnloadRenderTexture(EditorRender.OutlineRt);
                EditorRender.OutlineRt = LoadRenderTexture((int)EditorRender.TexSize.X, (int)EditorRender.TexSize.Y);
                SetTextureWrap(EditorRender.OutlineRt.Texture, TextureWrap.Clamp);

                EditorRender.TexTemp = EditorRender.TexSize;
            }

            if (RuntimeRender.TexSize != RuntimeRender.TexTemp) {
                UnloadRenderTexture(RuntimeRender.Rt);
                RuntimeRender.Rt = Core.LoadRenderTextureWithDepth((int)RuntimeRender.TexSize.X, (int)RuntimeRender.TexSize.Y);

                RuntimeRender.TexTemp = RuntimeRender.TexSize;
            }

            // Outline mask pass
            if (LevelBrowser.SelectedObject != null || Picking.DragSource != null || Picking.DragTarget != null) {
                BeginTextureMode(EditorRender.OutlineRt);
                ClearBackground(Color.Blank);
                ClearScreenBuffers();
                BeginMode3D(_editorCamera.Raylib);
                foreach (var obj in LevelBrowser.SelectedObjects) RenderOutline(obj);
                if (Picking.DragSource != null) RenderOutline(Picking.DragSource);
                if (Picking.DragTarget != null) RenderOutline(Picking.DragTarget);
                EndMode3D();
                EndTextureMode();
            }

            // Runtime viewport
            BeginTextureMode(RuntimeRender.Rt);
            Window.Clear(Colors.Game);
            Core.IsPreviewRender = true;

            // 3D Pass
            if (Core.GameCamera != null) {
                BeginMode3D(Core.GameCamera.Raylib);
                PostProcessing.ApplyJitter(Core.GameCamera);
                Core.LastProjectionMatrix = Rlgl.GetMatrixProjection();
                Core.LastViewMatrix       = Rlgl.GetMatrixModelview();
                Core.Render(false);
                EndMode3D();
            }

            EndTextureMode();

            // Post-Process Pass
            PostProcessing.Apply(RuntimeRender.Rt);

            // 2D Pass
            BeginTextureMode(RuntimeRender.Rt);
            Core.Render(true);
            Core.IsPreviewRender = false;
            EndTextureMode();

            // Editor viewport
            BeginTextureMode(EditorRender.Rt);
            Window.Clear(Colors.Game);

            Core.ActiveCamera = _editorCamera;
            FreeCam.Loop(EditorRender);

            Camera.ApplySettings(_editorCamera, 0.01f, 2000.0f);
            BeginMode3D(_editorCamera.Raylib);
            Core.LastProjectionMatrix = Rlgl.GetMatrixProjection();
            Core.LastViewMatrix       = Rlgl.GetMatrixModelview();
            Core.Render(false);
            Grid.Draw(_editorCamera);
            EndMode3D();

            // Post-process outline
            if (LevelBrowser.SelectedObject != null || Picking.IsDragging) {
                var outlinePost = AssetManager.Get<ShaderAsset>("outline_post");

                if (outlinePost != null) {
                    BeginShaderMode(outlinePost.Shader);
                    SetShaderValue(outlinePost.Shader, outlinePost.GetLoc("textureSize"),  new Vector2(EditorRender.TexSize.X, EditorRender.TexSize.Y), ShaderUniformDataType.Vec2);
                    SetShaderValue(outlinePost.Shader, outlinePost.GetLoc("outlineSize"),  2.0f,                                                        ShaderUniformDataType.Float);
                    SetShaderValue(outlinePost.Shader, outlinePost.GetLoc("outlineColor"), ColorNormalize(Colors.Primary),                              ShaderUniformDataType.Vec4);
                    DrawTextureRec(EditorRender.OutlineRt.Texture, new Rectangle(0, 0, EditorRender.TexSize.X, -EditorRender.TexSize.Y), Vector2.Zero, Color.White);
                    EndShaderMode();
                }
            }

            Core.Render(true); // 2D Icons/Gizmos
            Picking.Render2D();
            EndTextureMode();

            // ImGui
            MenuBar.Draw();
            EditorRender.Draw();
            RuntimeRender.Draw();
            LevelBrowser.Draw();
            ObjectBrowser.Draw();
            ProjectBrowser.Draw();
            ScriptEditor.Draw();
            MusicPlayer.Draw();
            Preview.Draw();

            Picking.Update();

            // END
            PopFont();
            Style.Pop();
            rlImGui.End();

            Notifications.Draw();
            EndDrawing();

            Shortcuts.Check();

            if (_scheduledQuit) break;
        }

        ViewSettings.Save();
        MusicPlayer.Save();
        ScriptEditor.Save();
        ProjectBrowser.Save();
        EditorRender.Save();

        Shutdown();
        Core.Quit();
        CloseWindow();

        SafeExec.Try(() => Directory.Delete(PathUtil.TempPath, true));
    }

    public static void Quit() => _scheduledQuit = true;

    public static void TogglePlayMode(Vector2? mouseCenter = null) {
        if (Core.ActiveLevel == null) return;

        if (!Core.IsPlaying) {
            // Isolate editor state
            _editorLevelRef = Core.ActiveLevel;
            var snapshot = Core.ActiveLevel.ToSnapshot();

            Core.IsPlaying    = true;
            LuaMouse.IsLocked = true;
            LuaTime.Reset();
            RuntimeRender.IsOpen      = true;
            RuntimeRender.ShouldFocus = true;

            // Re-init physics to clear any leftovers and prepare for fresh simulation
            Physics.Init();

            // Replace the level reference in the active slot with a runtime clone
            Core.OpenLevels[Core.ActiveLevelIndex] = new Level(_editorLevelRef.Name, _editorLevelRef.JsonPath, snapshot);
            Core.SetActiveLevel(Core.ActiveLevelIndex, clearHistory: false);
            Core.Load();

            if (mouseCenter.HasValue) SetMousePosition((int)mouseCenter.Value.X, (int)mouseCenter.Value.Y);

            Notifications.Show("Play Mode Started");
        } else {
            // Stop play mode
            Core.IsPlaying    = false;
            LuaMouse.IsLocked = false;
            EnableCursor();
            ShowCursor();

            // Re-init physics to clear runtime bodies
            Physics.Init();

            // Restore - Undo history holds references to objects in _editorLevelRef.
            if (_editorLevelRef != null) {
                Core.OpenLevels[Core.ActiveLevelIndex] = _editorLevelRef;
                Core.SetActiveLevel(Core.ActiveLevelIndex, clearHistory: false);

                // Force reload rigidbodies because Physics World was reset
                ReloadPhysics(_editorLevelRef.Root);

                _editorLevelRef = null;
            }

            Notifications.Show("Play Mode Stopped");
        }

        Core.ActiveCamera = Core.IsPlaying ? Core.GameCamera : _editorCamera;
    }

    private static void ReloadPhysics(Obj obj) {
        if (obj.Components.TryGetValue("Rigidbody", out var rb)) {
            rb.IsLoaded = false;
            rb.Load();
            rb.IsLoaded = true;
        }

        foreach (var child in obj.Children.Values) ReloadPhysics(child);
    }

    private static void RenderOutline(Obj obj) {
        foreach (var component in obj.Components.Values) {
            if (component is not Model { IsLoaded: true } model) continue;

            // Override shaders
            var modelAsset  = model.AssetRef;
            var outlineMask = AssetManager.Get<ShaderAsset>("outline_mask");

            if (outlineMask != null) {
                // Track original shaders by Material index to handle shared materials correctly
                var originalShaders = new Dictionary<int, Shader>();

                for (var i = 0; i < modelAsset.Materials.Length; i++) {
                    originalShaders[i]             = modelAsset.Materials[i].Shader;
                    modelAsset.Materials[i].Shader = outlineMask.Shader;
                }

                model.Draw();

                // Restore
                for (var i = 0; i < modelAsset.Materials.Length; i++)
                    if (originalShaders.TryGetValue(i, out var shader))
                        modelAsset.Materials[i].Shader = shader;
            } else
                model.Draw();
        }

        foreach (var child in obj.Children.Values) RenderOutline(child);
    }
}