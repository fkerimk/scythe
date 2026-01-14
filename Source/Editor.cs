using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

internal unsafe class Editor() : RaylibSession(1, 1, [ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow], true) {

    private ImGuiIOPtr _io;
    
    private Level3D? _level3D;
    private LevelBrowser? _levelBrowser;
    private ObjectBrowser? _objectBrowser;
    private ProjectBrowser? _projectBrowser;

    public Core Core;
    public FreeCam FreeCam;
    
    protected override void Init() {

        ResizeWindow(new int2(Screen.Width / 2, Screen.Height / 2));
        CenterWindow();
        
        // ImGui setup
        rlImGui.Setup(true, true);
        
        _io = ImGui.GetIO();
        _io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        _io.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi("Layouts/User.ini").ToPointer();
        
        // Generic initials
        Core = new Core(true, new Cam());
        Core.ActiveLevel = new Level("Main", Core);
        FreeCam = new FreeCam(Core.ActiveCamera);
        Fonts.LoadImFonts(_io);

        if (Core.ActiveLevel == null) return;
        
        // Viewports
        _level3D = new Level3D { CustomStyle = new CustomStyle {
            WindowPadding = new Vector2(0, 0),
            CellPadding = new Vector2(0, 0),
            SeparatorTextPadding = new Vector2(0, 0)
        }};
        
        _levelBrowser = new LevelBrowser(this);
        _objectBrowser = new ObjectBrowser();
        _projectBrowser = new ProjectBrowser();
    }

    protected override bool Loop() {
        
        TargetFps = Config.Editor.FpsLock;
        
        if (_level3D == null ||
            _levelBrowser == null ||
            _objectBrowser == null ||
            _projectBrowser == null
        ) return false;
        
        // Reload viewport render
        if (_level3D.TexSize != _level3D.TexTemp) {
                
            Raylib.UnloadRenderTexture(_level3D.Rt);
            _level3D.Rt = Raylib.LoadRenderTexture((int)_level3D.TexSize.X, (int)_level3D.TexSize.Y);
            _level3D.TexTemp = _level3D.TexSize;
        }
            
        // Render core on viewport
        Raylib.BeginTextureMode(_level3D.Rt);
            
        Clear(Colors.Game);

        // Start camera
        FreeCam.Loop(_level3D);
        
        Core.ActiveCamera.StartRendering();
            
        // 3D
        var grid = new Grid(Core.ActiveCamera);
        grid.Draw();
            
        Core.Loop3D(true);
        Core.Loop3DEditor(_level3D);
            
        // Stop camera
        Core.ActiveCamera.StopRendering();
            
        // Ui
        Core.LoopUi(true);
        Core.LoopUiEditor(_level3D);
            
        if (Config.Editor.DrawFps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, Colors.Primary.ToRaylib());

        Raylib.EndTextureMode();
            
        // Draw raylib
        Raylib.BeginDrawing();
        Clear(new Color(0, 0, 0));
            
        // Draw ImGui
        rlImGui.Begin();
        Style.Push();
        ImGui.PushFont(Fonts.ImMontserratRegular);
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
        _io.MouseDoubleClickTime = 0.2f;
            
        // Draw elements
        _level3D.Draw();
        _levelBrowser.Draw();
        _objectBrowser.Obj = _levelBrowser.SelectedObject;
        _objectBrowser.Draw();
        _projectBrowser.Draw();
        
        // Stop ImGui
        ImGui.PopFont();
        Style.Pop();
        rlImGui.End();

        Notifications.Draw();
        
        // Shortcuts
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl)) {
        
            if (Raylib.IsKeyPressed(KeyboardKey.D)) {

                if (_levelBrowser.SelectedObject != null) {

                    Core.ActiveLevel?.RecordedCloneObject(_levelBrowser.SelectedObject);
                }
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.S)) {

                Core.ActiveLevel?.Save();
                Notifications.Show("Saved");
            }
            
            if (Raylib.IsKeyPressed(KeyboardKey.Z)) History.Undo();
            if (Raylib.IsKeyPressed(KeyboardKey.Y)) History.Redo();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Delete)) {

            if (_levelBrowser.SelectedObject != null)
                _levelBrowser.DeleteObject = _levelBrowser.SelectedObject;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.F5)) {

            var currentPath = Process.GetCurrentProcess().MainModule?.FileName;

            if (!string.IsNullOrEmpty(currentPath)) {
                
                Console.WriteLine(currentPath);

                var psi = new ProcessStartInfo {
                    
                    FileName = currentPath,
                    Arguments = "-no-splash",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = PathUtil.FirstDir
                };

                using var process = Process.Start(psi);
                process?.WaitForExit();
            }
        }

        return !Raylib.WindowShouldClose();
    }
    
    protected override void Quit() {
        
        Core.Quit();
    }
}