using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using static ImGuiNET.ImGui;
using static Raylib_cs.Raylib;
using static rlImGui_cs.rlImGui;

internal static unsafe class Editor {

    public static ImGuiIOPtr ImGuiIoPtr;
    
    public static Level3D? Level3D;
    public static LevelBrowser? LevelBrowser;
    public static ObjectBrowser? ObjectBrowser;
    public static ProjectBrowser? ProjectBrowser;
    
    public static void Show() {
        
        Window.Show(scale: 0.75f, flags: [ ConfigFlags.Msaa4xHint, ConfigFlags.ResizableWindow ]);

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
        
        // Viewports
        Level3D = new Level3D { CustomStyle = new CustomStyle {
            WindowPadding = new Vector2(0, 0),
            CellPadding = new Vector2(0, 0),
            SeparatorTextPadding = new Vector2(0, 0)
        }};
        
        LevelBrowser = new LevelBrowser();
        ObjectBrowser = new ObjectBrowser();
        ProjectBrowser = new ProjectBrowser();

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
            Core.Loop3D();
            Core.Loop3DEditor(Level3D);
            EndMode3D();
            Core.LoopUi();
            Core.LoopUiEditor(Level3D);
            if (Config.Editor.DrawFps) DrawText($"{GetFPS()}", 10, 10, 20, Colors.Primary.ToRaylib());
            EndTextureMode();
            
            // Render editor
            BeginDrawing();
            ClearBackground(Color.Black);
            Begin();
            Style.Push();
            PushFont(Fonts.ImMontserratRegular);
            DockSpaceOverViewport(GetMainViewport().ID);
            ImGuiIoPtr.MouseDoubleClickTime = 0.2f;
            Level3D.Draw();
            LevelBrowser.Draw();
            ObjectBrowser.Obj = LevelBrowser.SelectedObject;
            ObjectBrowser.Draw();
            ProjectBrowser.Draw();
            PopFont();
            Style.Pop();
            rlImGui.End();
            Notifications.Draw();
            EndDrawing();
            
            // Shortcuts
            if (IsKeyDown(KeyboardKey.LeftControl)) {
        
                if (IsKeyPressed(KeyboardKey.D)) {

                    if (LevelBrowser.SelectedObject != null) {

                        Core.ActiveLevel?.RecordedCloneObject(LevelBrowser.SelectedObject);
                    }
                }
            
                if (IsKeyPressed(KeyboardKey.S)) {

                    Core.ActiveLevel?.Save();
                    Notifications.Show("Saved");
                }
            
                if (IsKeyPressed(KeyboardKey.Z)) History.Undo();
                if (IsKeyPressed(KeyboardKey.Y)) History.Redo();
            }

            if (IsKeyPressed(KeyboardKey.Delete)) {

                if (LevelBrowser.SelectedObject != null)
                    LevelBrowser.DeleteObject = LevelBrowser.SelectedObject;
            }

            if (IsKeyPressed(KeyboardKey.F5)) {

                var currentPath = Process.GetCurrentProcess().MainModule?.FileName;

                if (!string.IsNullOrEmpty(currentPath)) {
                
                    var psi = new ProcessStartInfo {
                    
                        FileName = currentPath,
                        Arguments = "nosplash",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = PathUtil.LaunchPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };
                
                    using var process = new Process();
                
                    process.StartInfo = psi;
                    process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

                    process.Start();
                
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                
                    process.WaitForExit();
                }
            }
        }
        
        CloseWindow();
        Core.Quit();
    }
}