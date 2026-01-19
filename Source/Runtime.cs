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
            
            // Input first
            LuaMouse.Loop();
            
            // Logic (updates matrices, scripts, and camera)
            Core.Logic();
            
            BeginDrawing();
            ClearBackground(Colors.Game.ToRaylib());
            
            // Shadow pass (internal RT switch)
            Core.ShadowPass();
            
            // Main Render
            BeginMode3D(Core.ActiveCamera.Raylib);
            Core.Render(false);
            EndMode3D();
            
            Core.Render(true);
            Window.DrawFps();
            EndDrawing();
            
            if (_scheduledQuit) break;
        }
        
        CloseWindow();
        Core.Quit();
    }
    
    public static void Quit() => _scheduledQuit = true;
}