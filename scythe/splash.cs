using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
internal static class splash {

    private const float duration = 2;
    private static readonly int2 size = new(320, 190);
    
    internal static void show() {
        
        Raylib.SetTraceLogLevel(TraceLogLevel.None);
        
        Raylib.InitWindow(size.x, size.y, "SCYTHE");
        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);

        var art = Raylib.LoadTexture(path.relative("art/splash.png"));

        var time = 0f;
        
        while (!Raylib.WindowShouldClose()) {

            Raylib.BeginDrawing();
                
            Raylib.SetWindowSize(size.x, size.y);
                
            Raylib.DrawTexturePro(art, new(0, 0, art.Width, art.Height), new(0, 0, size.x, size.y), Vector2.Zero, 0, Color.White);

            time += Raylib.GetFrameTime();

            if (time >= duration) break;
                
            Raylib.EndDrawing();
        }
            
        Raylib.CloseWindow();
    }
}