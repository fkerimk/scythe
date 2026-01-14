using Raylib_cs;

internal class Runtime() : RaylibSession(1, 1, [ ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow ], false) {

    public static Core Core;
    
    protected override void Init() {
        
        ResizeWindow(new int2(Screen.Width / 2, Screen.Height / 2));
        CenterWindow();

        Core = new Core(false, new Cam());
        Core.ActiveLevel = new Level("Main", Core);
    }

    protected override bool Loop() {
        
        TargetFps = Config.Runtime.FpsLock;
        
        Raylib.BeginDrawing();
            
        Clear(Colors.Game);
            
        Core.ActiveCamera.StartRendering();
            
        Core.Loop3D(false);
            
        Core.ActiveCamera.StopRendering();
            
        Core.LoopUi(false);

        if (Config.Runtime.DrawFps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, Colors.Primary.ToRaylib());

        return !Raylib.WindowShouldClose();
    }

    protected override void Quit() {
        
        Core.Quit();
    }
}