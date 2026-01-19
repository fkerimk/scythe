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

    private static readonly LevelBrowser LevelBrowser = new();
    private static readonly ObjectBrowser ObjectBrowser = new();
    private static readonly ProjectBrowser ProjectBrowser = new();
    
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

            // Render core on viewport
            BeginTextureMode(Level3D.Rt);
            Window.Clear(Colors.Game);
            FreeCam.Loop(Level3D);
            BeginMode3D(Core.ActiveCamera.Raylib);
            Grid.Draw(Core.ActiveCamera);
            Core.Loop(false);
            
            // CRITICAL: Shadow mapping inside Core.Loop(false) calls EndTextureMode().
            // We MUST return to the viewport RT for the rest of the frame.
            BeginTextureMode(Level3D.Rt);
            
            EndMode3D();
            Core.Loop(true);
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

        CloseWindow();
        Core.Quit();
    }

    public static void Quit() => _scheduledQuit = true;
}

