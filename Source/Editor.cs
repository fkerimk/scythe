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

    public static readonly Level3D Level3D = new() { CustomStyle = new CustomStyle {
        
        WindowPadding = new Vector2(0, 0),
        CellPadding = new Vector2(0, 0),
        SeparatorTextPadding = new Vector2(0, 0)
    }};

    public static readonly LevelBrowser LevelBrowser = new();
    public static readonly ProjectBrowser ProjectBrowser = new();
    
    private static readonly ObjectBrowser ObjectBrowser = new();
    
    public static void Show() {
        
        Window.Show(flags: [ ConfigFlags.Msaa4xHint, ConfigFlags.ResizableWindow ], title: $"{Config.Mod.Name} - Editor");

        // Setup ImGui
        Setup(true, true);
        
        var layoutPath = PathUtil.ExeRelative("Layouts/User.ini");
        
        if (PathUtil.BestPath("Layouts/User.ini", out var existLayoutPath))
            layoutPath = existLayoutPath;
        
        ImGuiIoPtr = GetIO();
        ImGuiIoPtr.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        ImGuiIoPtr.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi(layoutPath).ToPointer();
        
        // Setup core
        Core.Init();
        FreeCam.SetFromTarget(Core.ActiveCamera);
        
        ViewSettings.Load();
        
        while (!WindowShouldClose()) {

            if (Core.ActiveCamera == null) break;

            Window.UpdateFps();
            Core.Load();

            // Reload viewport render
            if (Level3D.TexSize != Level3D.TexTemp) {

                UnloadRenderTexture(Level3D.Rt);
                Level3D.Rt = LoadRenderTexture((int)Level3D.TexSize.X, (int)Level3D.TexSize.Y);
                
                UnloadRenderTexture(Level3D.OutlineRt);
                Level3D.OutlineRt = LoadRenderTexture((int)Level3D.TexSize.X, (int)Level3D.TexSize.Y);
                SetTextureWrap(Level3D.OutlineRt.Texture, TextureWrap.Clamp);
                
                Level3D.TexTemp = Level3D.TexSize;
            }

            // Core Logic & Shadow Mapping (Internal switches RT, ends in screen buffer)
            Core.Logic();
            Core.ShadowPass();

            // Outline Mask Pass
            if (LevelBrowser.SelectedObject != null) {
            
                BeginTextureMode(Level3D.OutlineRt);
                ClearBackground(Color.Blank);
                ClearScreenBuffers(); // Explicitly clear depth and color buffers
                
                BeginMode3D(Core.ActiveCamera.Raylib);
                RenderOutline(LevelBrowser.SelectedObject);
                EndMode3D();
                
                EndTextureMode();
            }

            // Viewport Rendering
            BeginTextureMode(Level3D.Rt);
            Window.Clear(Colors.Game);
            
            FreeCam.Loop(Level3D);
            
            BeginMode3D(Core.ActiveCamera.Raylib);
            Core.Render(false); // Draw objects and skybox
            Grid.Draw(Core.ActiveCamera);
            EndMode3D();
            
            // Render Outline Post
            if (LevelBrowser.SelectedObject != null) {
                 
                BeginShaderMode(Shaders.OutlinePost);
                SetShaderValue(Shaders.OutlinePost, Shaders.OutlineTextureSize, new Vector2(Level3D.TexSize.X, Level3D.TexSize.Y), ShaderUniformDataType.Vec2);
                SetShaderValue(Shaders.OutlinePost, Shaders.OutlineWidth, 2.0f, ShaderUniformDataType.Float);
                SetShaderValue(Shaders.OutlinePost, Shaders.OutlineColor, ColorNormalize(Colors.Primary), ShaderUniformDataType.Vec4);
                DrawTextureRec(Level3D.OutlineRt.Texture, new Rectangle(0, 0, Level3D.TexSize.X, -Level3D.TexSize.Y), Vector2.Zero, Color.White);
                EndShaderMode();
            }
            
            Core.Render(true); // Draw 2D UI for components
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
            Level3D.Draw();
            Picking.Update();
            LevelBrowser.Draw();
            ObjectBrowser.Draw();
            ProjectBrowser.Draw();
            PopFont();
            Style.Pop();
            rlImGui.End();
            Notifications.Draw();
            EndDrawing();

            Shortcuts.Check();
            
            if (_scheduledQuit) break;
        }

        ViewSettings.Save();
        CloseWindow();

        
        // Cleanup temp
        try {
            var tempPath = PathUtil.ExeRelative("Temp");
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        } catch { /**/ }

        Core.Quit();
    }

    public static void Quit() => _scheduledQuit = true;

    private static void RenderOutline(Obj obj) {

        foreach (var component in obj.Components.Values) {
            
            if (component is not Model model) continue;
            
            // Override shaders
            var originalShaders = new Shader[model.RlModel.MaterialCount];
            
            for (var i = 0; i < model.RlModel.MaterialCount; i++) {
                
                originalShaders[i] = model.RlModel.Materials[i].Shader;
                model.RlModel.Materials[i].Shader = Shaders.OutlineMask;
            } 
               
            // Draw
            model.Draw();
               
            // Restore
            for (var i = 0; i < model.RlModel.MaterialCount; i++)
                model.RlModel.Materials[i].Shader = originalShaders[i];
        }
        
        foreach (var child in obj.Children.Values) RenderOutline(child);
    }
}

