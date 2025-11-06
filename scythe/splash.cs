using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
internal class splash(float duration) : raylib_session{
    protected override void draw() {
        
        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);

        var art = Raylib.LoadTexture(path.relative("art/splash.png"));

        var time = 0f;
        
        while (!Raylib.WindowShouldClose()) {

            Raylib.BeginDrawing();
                
            Raylib.SetWindowSize(width, height);
                
            Raylib.DrawTexturePro(art, new(0, 0, art.Width, art.Height), new(0, 0, width, height), Vector2.Zero, 0, Color.White);

            time += Raylib.GetFrameTime();

            if (time >= duration) break;
                
            Raylib.EndDrawing();
        }
            
        Raylib.CloseWindow();
    }
}