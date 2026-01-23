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
    
    private static bool _scheduledQuit;
    
    public static ImGuiIOPtr ImGuiIoPtr;

    public static EditorRender EditorRender = null!;
    public static LevelBrowser LevelBrowser = null!;
    public static ProjectBrowser ProjectBrowser = null!;
    // ReSharper disable once MemberCanBePrivate.Global
    public static ObjectBrowser ObjectBrowser = null!;
    // ReSharper disable once MemberCanBePrivate.Global
    public static ScriptEditor ScriptEditor = null!;
    // ReSharper disable once MemberCanBePrivate.Global
    public static MusicPlayer MusicPlayer = null!;
    
    public static bool IsScriptEditorFocused => ScriptEditor.IsFocused;
    
    public static void OpenScript(string path) => ScriptEditor.Open(path);
    
    public static void Show() {
        
        Window.Show(flags: [ ConfigFlags.Msaa4xHint, ConfigFlags.ResizableWindow ], title: $"{Config.Mod.Name} - Editor");

        // Setup ImGui
        Setup(true, true);

        EditorRender = new EditorRender {
            
            CustomStyle = new CustomStyle {
                
                WindowPadding = new Vector2(0, 0),
                CellPadding = new Vector2(0, 0),
                SeparatorTextPadding = new Vector2(0, 0)
            }
        };

        LevelBrowser = new LevelBrowser();
        ProjectBrowser = new ProjectBrowser();
        ObjectBrowser = new ObjectBrowser();
        ScriptEditor = new ScriptEditor();
        MusicPlayer = new MusicPlayer();

        var layoutPath = PathUtil.ExeRelative("Layouts/User.ini");
        
        if (PathUtil.BestPath("Layouts/User.ini", out var existLayoutPath))
            layoutPath = existLayoutPath;
        
        ImGuiIoPtr = GetIO();
        ImGuiIoPtr.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGuiIoPtr.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi(layoutPath).ToPointer();
        
        // Setup core
        Core.Init();
        
        if (Core.ActiveLevel?.EditorCamera == null)
            FreeCam.SetFromTarget(Core.ActiveCamera);
        
        ViewSettings.Load();
        MusicPlayer.Load();
        ScriptEditor.Load();
        ProjectBrowser.Load();
        
        while (!WindowShouldClose()) {

            if (Core.ActiveCamera == null) break;

            Window.UpdateFps();
            Core.Load();

            // Reload viewport render
            if (EditorRender.TexSize != EditorRender.TexTemp) {

                UnloadRenderTexture(EditorRender.Rt);
                EditorRender.Rt = LoadRenderTexture((int)EditorRender.TexSize.X, (int)EditorRender.TexSize.Y);
                
                UnloadRenderTexture(EditorRender.OutlineRt);
                EditorRender.OutlineRt = LoadRenderTexture((int)EditorRender.TexSize.X, (int)EditorRender.TexSize.Y);
                SetTextureWrap(EditorRender.OutlineRt.Texture, TextureWrap.Clamp);
                
                EditorRender.TexTemp = EditorRender.TexSize;
            }

            // Core Logic & Shadow Mapping (Internal switches RT, ends in screen buffer)
            Core.Logic();
            Core.ShadowPass();

            // Outline Mask Pass
            if (LevelBrowser.SelectedObject != null || Picking.DragSource != null || Picking.DragTarget != null) {
            
                BeginTextureMode(EditorRender.OutlineRt);
                ClearBackground(Color.Blank);
                ClearScreenBuffers(); // Explicitly clear depth and color buffers
                
                BeginMode3D(Core.ActiveCamera.Raylib);
                foreach (var obj in LevelBrowser.SelectedObjects) RenderOutline(obj);
                if (Picking.DragSource != null) RenderOutline(Picking.DragSource);
                if (Picking.DragTarget != null) RenderOutline(Picking.DragTarget);
                EndMode3D();
                
                EndTextureMode();
            }

            // Viewport Rendering
            BeginTextureMode(EditorRender.Rt);
            Window.Clear(Colors.Game);
            
            // Freecam
            FreeCam.Loop(EditorRender);
            
            // Draw objects and skybox
            BeginMode3D(Core.ActiveCamera.Raylib);
            Core.Render(false);
            Grid.Draw(Core.ActiveCamera);
            EndMode3D();
            
            // Render Outline Post
            if (LevelBrowser.SelectedObject != null || Picking.IsDragging) {
                 
                BeginShaderMode(Shaders.OutlinePost);
                SetShaderValue(Shaders.OutlinePost, Shaders.OutlineTextureSize, new Vector2(EditorRender.TexSize.X, EditorRender.TexSize.Y), ShaderUniformDataType.Vec2);
                SetShaderValue(Shaders.OutlinePost, Shaders.OutlineWidth, 2.0f, ShaderUniformDataType.Float);
                SetShaderValue(Shaders.OutlinePost, Shaders.OutlineColor, ColorNormalize(Colors.Primary), ShaderUniformDataType.Vec4);
                DrawTextureRec(EditorRender.OutlineRt.Texture, new Rectangle(0, 0, EditorRender.TexSize.X, -EditorRender.TexSize.Y), Vector2.Zero, Color.White);
                EndShaderMode();
            }
            
            // Draw 2D UI for components
            Core.Render(true);
            Picking.Render2D();
            Window.DrawFps();
            EndTextureMode();

            // Render editor
            BeginDrawing();
            ClearBackground(Color.Black);
            Begin();
            Style.Push();
            PushFont(Fonts.ImMontserratRegular);
            DockSpaceOverViewport(GetMainViewport().ID);
            ImGuiIoPtr.MouseDoubleClickTime = 0.2f;
            MenuBar.Draw();
            EditorRender.Draw();
            LevelBrowser.Draw();
            ObjectBrowser.Draw();
            ProjectBrowser.Draw();
            ScriptEditor.Draw();
            MusicPlayer.Draw();
            Picking.Update();
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
        CloseWindow();

        Directory.Delete(PathUtil.TempPath, true);

        Core.Quit();
    }

    public static void Quit() => _scheduledQuit = true;

    private static void RenderOutline(Obj obj) {

        foreach (var component in obj.Components.Values) {
            
            if (component is not Model { IsLoaded: true } model) continue;
            
            // Override shaders
            var originalShaders = new Shader[model.Asset.RlModel.MaterialCount];
            
            for (var i = 0; i < model.Asset.RlModel.MaterialCount; i++) {
                
                originalShaders[i] = model.Asset.RlModel.Materials[i].Shader;
                model.Asset.RlModel.Materials[i].Shader = Shaders.OutlineMask;
            } 
               
            // Draw
            model.Draw();
               
            // Restore
            for (var i = 0; i < model.Asset.RlModel.MaterialCount; i++)
                model.Asset.RlModel.Materials[i].Shader = originalShaders[i];
        }
        
        foreach (var child in obj.Children.Values) RenderOutline(child);
    }
}