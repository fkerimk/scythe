using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using static ImGuiNET.ImGui;
using static Raylib_cs.Raylib;
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
    public static readonly ObjectBrowser ObjectBrowser = new();
    public static readonly ProjectBrowser ProjectBrowser = new();
    
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
                Level3D.TexTemp = Level3D.TexSize;
            }

            // Core Logic & Shadow Mapping (Internal switches RT, ends in screen buffer)
            Core.Logic();
            Core.ShadowPass();

            // Viewport Rendering
            BeginTextureMode(Level3D.Rt);
            Window.Clear(Colors.Game);
            
            FreeCam.Loop(Level3D);
            
            BeginMode3D(Core.ActiveCamera.Raylib);
            Core.Render(false); // Draw objects and skybox
            Grid.Draw(Core.ActiveCamera);
            EndMode3D();
            
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
        Core.Quit();
    }

    public static void Quit() => _scheduledQuit = true;
}

