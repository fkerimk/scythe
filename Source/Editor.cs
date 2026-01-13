using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Newtonsoft.Json;
using Raylib_cs;
using rlImGui_cs;

internal unsafe class Editor() : RaylibSession(1, 1, ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow) {

    private ImGuiIOPtr _io;
    
    private Level3D? _level3D;
    private LevelBrowser? _levelBrowser;
    private ObjectBrowser? _objectBrowser;
    private ProjectBrowser? _projectBrowser;
    private InsertBox? _insertBox;
    
    protected override void init() {
        
        resize_window(new int2(Screen.Width / 2, Screen.Height / 2));
        center_window();
        
        // ImGui setup
        rlImGui.Setup(true, true);
        
        _io = ImGui.GetIO();
        _io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        _io.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi("Layouts/User.ini").ToPointer();
        
        // Generic initials
        Core.Init(true);
        FreeCam.Init();
        Fonts.Load(_io);

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

    protected override void loop() {
        
        TargetFps = Config.Editor.FpsLock;
        
        if (Cam.Main == null ||
            _level3D == null ||
            _levelBrowser == null ||
            _objectBrowser == null ||
            _projectBrowser == null ||
            _insertBox == null
        ) return;
        
        // reload viewport render
        if (_level3D.TexSize != _level3D.TexTemp) {
                
            Raylib.UnloadRenderTexture(_level3D.Rt);
            _level3D.Rt = Raylib.LoadRenderTexture((int)_level3D.TexSize.X, (int)_level3D.TexSize.Y);
            _level3D.TexTemp = _level3D.TexSize;
        }
            
        // render core on viewport
        Raylib.BeginTextureMode(_level3D.Rt);
            
        clear(Colors.Game);

        // start camera
        FreeCam.Loop(_level3D);
        
        Cam.Main.StartRendering();
            
        // 3d
        var grid = new Grid(Cam.Main);
        grid.Draw();
            
        Core.Loop3D(true);
        Core.Loop3DEditor(_level3D);
            
        // stop camera
        Cam.Main.StopRendering();
            
        // ui
        Core.LoopUi(true);
        Core.LoopUiEditor(_level3D);
            
        if (Config.Editor.DrawFps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, Colors.Primary.to_raylib());

        Raylib.EndTextureMode();
            
        // draw raylib
        Raylib.BeginDrawing();
        clear(new Color(0, 0, 0));
            
        // Draw ImGui
        rlImGui.Begin();
        Style.Push();
        ImGui.PushFont(Fonts.MontserratRegular);
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
        _io.MouseDoubleClickTime = 0.2f;
            
        // Draw elements
        _level3D.Draw();
        _levelBrowser.Draw();
        _objectBrowser.obj = _levelBrowser.SelectedObject;
        _objectBrowser.Draw();
        _projectBrowser.Draw();
        _insertBox.Draw();
        
        // Stop i
        ImGui.PopFont();
        Style.Pop();
        rlImGui.End();
        
        // shortcuts
        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) &&  Raylib.IsKeyPressed(KeyboardKey.S)) {

            Core.ActiveLevel?.Save();
        }
    }

    protected override void quit() {
        
        Core.Quit();
    }
}