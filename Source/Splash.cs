using System.Numerics;
using Raylib_cs;

internal class Splash(float duration) : RaylibSession(320, 190, ConfigFlags.UndecoratedWindow) {

    private float _time;
    private Texture2D _art;
    
    protected override void init() {
        
        Raylib.SetWindowState(ConfigFlags.UndecoratedWindow);
        _art = Raylib.LoadTexture(PathUtil.Relative("Images/Splash.png"));
    }

    protected override void loop() {
        
        Raylib.BeginDrawing();
                
        Raylib.SetWindowSize(Width, Height);
                
        Raylib.DrawTexturePro(_art, new Rectangle(0, 0, _art.Width, _art.Height), new Rectangle(0, 0, Width, Height), Vector2.Zero, 0, Raylib_cs.Color.White);

        _time += Raylib.GetFrameTime();

        if (_time >= duration) CloseWindow = true;
    }

    protected override void quit() {}
}