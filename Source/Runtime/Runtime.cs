using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Runtime {

    private static bool _scheduledQuit;
    
    public static void Show() {
        
        Window.Show(fullscreen: false, flags: [ ConfigFlags.Msaa4xHint, ConfigFlags.ResizableWindow ], title: Config.Mod.Name);
        
        // Setup core
        Core.Init();
        
        while (!WindowShouldClose()) {
            
            if (Core.ActiveCamera == null) break;
            
            Window.UpdateFps();
            Core.Load();
            BeginDrawing();
            ClearBackground(Colors.Game.ToRaylib());
            BeginMode3D(Core.ActiveCamera.Raylib);
            Core.Loop(false);
            LuaMouse.Loop();
            EndMode3D();
            Core.Loop(true);
            Window.DrawFps();
            EndDrawing();
            
            if (_scheduledQuit) break;
        }
        
        CloseWindow();
        Core.Quit();
    }
    
    public static void Quit() => _scheduledQuit = true;
}