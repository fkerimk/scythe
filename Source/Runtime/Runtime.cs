using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Runtime {

    public static void Show() {
        
        Window.Show(fullscreen: true, flags: [ ConfigFlags.Msaa4xHint, ConfigFlags.ResizableWindow ], title: Config.Mod.Name);
        
        // Setup core
        Core.Init();

        while (!WindowShouldClose()) {
            
            if (Core.ActiveCamera == null) break;
            
            Window.UpdateFps();
            
            Core.Load();
            
            BeginDrawing();
            ClearBackground(Colors.Game.ToRaylib());
            BeginMode3D(Core.ActiveCamera.Raylib);
            Core.Loop3D();
            EndMode3D();
            Core.LoopUi();
            if (Config.Runtime.DrawFps) DrawText($"{GetFPS()}", 10, 10, 20, Colors.Primary.ToRaylib());
            EndDrawing();
        }
        
        CloseWindow();
        Core.Quit();
    }
}