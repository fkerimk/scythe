using System.Numerics;
using Raylib_cs;

internal class Splash(float duration) : RaylibSession(320, 190, [ ConfigFlags.UndecoratedWindow ]) {

    private float _time;
    private Texture2D _art;
    
    protected override bool Init() {
        
        if (!PathUtil.BestPath("Images/Splash.png", out var bestPath)) return false;
        
        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
        _art = Raylib.LoadTexture(bestPath);
            
        return true;
    }

    protected override bool Loop() {
        
        Raylib.BeginDrawing();
                
        Raylib.SetWindowSize(Width, Height);
                
        Raylib.DrawTexturePro(_art, new Rectangle(0, 0, _art.Width, _art.Height), new Rectangle(0, 0, Width, Height), Vector2.Zero, 0, Raylib_cs.Color.White);

        _time += Raylib.GetFrameTime();

        if (_time >= duration) return false;
        
        return true;
    }

    protected override void Quit() {}
}