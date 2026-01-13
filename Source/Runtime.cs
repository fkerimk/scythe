using Raylib_cs;

internal class Runtime() : RaylibSession(1, 1, ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow) {

    protected override void init() {
        
        resize_window(new int2(Screen.Width / 2, Screen.Height / 2));
        center_window();
        
        Core.Init(false);
    }

    protected override void loop() {
        
        TargetFps = Config.Runtime.FpsLock;
        
        if (Cam.Main == null) return;

        Raylib.BeginDrawing();
            
        clear(Colors.Game);
            
        Cam.Main.StartRendering();
            
        Core.Loop3D(false);
            
        Cam.Main.StopRendering();
            
        Core.LoopUi(false);

        if (Config.Runtime.DrawFps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, Colors.Primary.to_raylib());
    }

    protected override void quit() {
        
        Core.Quit();
    }
}