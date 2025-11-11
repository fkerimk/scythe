using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
internal class splash(float duration) : raylib_session(320, 190, TraceLogLevel.None, ConfigFlags.UndecoratedWindow) {

    private float time;
    private Texture2D art;
    
    protected override void init() {
        
        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
        art = Raylib.LoadTexture(path.relative("art/splash.png"));
    }

    protected override void loop() {
        
        Raylib.BeginDrawing();
                
        Raylib.SetWindowSize(width, height);
                
        Raylib.DrawTexturePro(art, new(0, 0, art.Width, art.Height), new(0, 0, width, height), Vector2.Zero, 0, Color.White);

        time += Raylib.GetFrameTime();

        if (time >= duration) close_window = true;
    }

    protected override void quit() {}
}