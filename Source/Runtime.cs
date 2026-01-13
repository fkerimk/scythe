using Raylib_cs;

internal class Runtime() : RaylibSession(1, 1, [ ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow ], false) {

    protected override void Init() {
        
        ResizeWindow(new int2(Screen.Width / 2, Screen.Height / 2));
        CenterWindow();
        
        Core.Init(false);
    }

    protected override void Loop() {
        
        TargetFps = Config.Runtime.FpsLock;
        
        if (Cam.Main == null) return;

        Raylib.BeginDrawing();
            
        Clear(Colors.Game);
            
        Cam.Main.StartRendering();
            
        Core.Loop3D(false);
            
        Cam.Main.StopRendering();
            
        Core.LoopUi(false);

        if (Config.Runtime.DrawFps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, Colors.Primary.to_raylib());
    }

    protected override void Quit() {
        
        Core.Quit();
    }
}