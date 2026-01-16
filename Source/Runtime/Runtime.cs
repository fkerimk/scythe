using Raylib_cs;

internal class Runtime() : RaylibSession(1, 1, [ ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow ], false) {
    
    private static Core? _core;
    
    protected override bool Init() {
        
        Raylib.SetWindowSize(Screen.Width / 2, Screen.Height / 2);
        CenterWindow();
        
        _core = new Core(false);
        _core.ActiveLevel = new Level("Main", _core);
        
        Fonts.Init(false);

        return true;
    }

    protected override bool Loop() {
        
        if (_core == null) return true;
        
        _core.Load(false);

        _core.ActiveCamera ??= _core.ActiveLevel?.FindType<Camera>()?.Cam;

        TargetFps = Config.Runtime.FpsLock;
        
        Raylib.BeginDrawing();
            
        Clear(Colors.Game);
            
        _core.ActiveCamera?.StartRendering();
            
        _core.Loop3D(false);
            
        _core.ActiveCamera?.StopRendering();
            
        _core.LoopUi(false);

        if (Config.Runtime.DrawFps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, Colors.Primary.ToRaylib());

        return !Raylib.WindowShouldClose();
    }

    protected override void Quit() => _core?.Quit();
}