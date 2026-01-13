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
    private InsertBox? _insertBox;
    
    protected override void Init() {

        ResizeWindow(new int2(Screen.Width / 2, Screen.Height / 2));
        CenterWindow();
        
        // ImGui setup
        rlImGui.Setup(true, true);
        
        _io = ImGui.GetIO();
        _io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        _io.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi("Layouts/User.ini").ToPointer();
        
        // Generic initials
        Core.Init(true);
        FreeCam.Init();
        Fonts.LoadImFonts(_io);

        if (Cam.Main == null) return;
        if (Core.ActiveLevel == null) return;
        
        // Viewports
        _level3D = new Level3D { CustomStyle = new CustomStyle {
            WindowPadding = new Vector2(0, 0),
            CellPadding = new Vector2(0, 0),
            SeparatorTextPadding = new Vector2(0, 0)
        }};
        
        _levelBrowser = new LevelBrowser();
        _objectBrowser = new ObjectBrowser();
        _projectBrowser = new ProjectBrowser();
        _insertBox = new InsertBox();
    }

    protected override void Loop() {
        
        TargetFps = Config.Editor.FpsLock;
        
        if (Cam.Main == null ||
            _level3D == null ||
            _levelBrowser == null ||
            _objectBrowser == null ||
            _projectBrowser == null ||
            _insertBox == null
        ) return;
        
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
        
        Cam.Main.StartRendering();
            
        // 3D
        var grid = new Grid(Cam.Main);
        grid.Draw();
            
        Core.Loop3D(true);
        Core.Loop3DEditor(_level3D);
            
        // Stop camera
        Cam.Main.StopRendering();
            
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
        _insertBox.Draw();
        
        // Stop ImGui
        ImGui.PopFont();
        Style.Pop();
        rlImGui.End();

        Notifications.Draw();
        
        // Shortcuts
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) &&  Raylib.IsKeyPressed(KeyboardKey.S)) {

            Core.ActiveLevel?.Save();
            Notifications.Show("Saved");
        }
        
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) &&  Raylib.IsKeyPressed(KeyboardKey.Z)) {

            History.Undo();
        }
        
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) &&  Raylib.IsKeyPressed(KeyboardKey.Y)) {

            History.Redo();
        }
    }

    protected override void Quit() {
        
        Core.Quit();
    }
}