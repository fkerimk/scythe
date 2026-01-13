using System.Numerics;
using Raylib_cs;

internal static class FreeCam {
    
    private const float Sens = 0.003f;
    private const float Clamp = 1.55f;
    private const float Speed = 15;
    
    private static Vector2 _rot;
    
    private static bool _isLocked;
    private static int2 _lockPos;

    public static void Init() {

        if (Cam.Main == null) return;
        
        set_from_target(Cam.Main.Pos, Cam.Main.Target);
    }
    
    public static void Loop(Viewport viewport) {
        
        var center = viewport.WindowPos.to_int2() + viewport.ContentRegion.to_int2() / 2;
        
        if (viewport.IsHovered && Raylib.IsMouseButtonPressed(MouseButton.Right)) {

            _isLocked = true;
            //lock_pos = Raylib.GetMousePosition();
            _lockPos = center;
            Raylib.DisableCursor();
        }

        if (_isLocked && Raylib.IsMouseButtonReleased(MouseButton.Right)) {
            
            Raylib.EnableCursor();
            Raylib.SetMousePosition(_lockPos.x, _lockPos.y);
            
            _isLocked = false;
        }
        
        if (!_isLocked) return;
        
        Movement();
        Rotation();
        
        Raylib.SetMousePosition(_lockPos.x, _lockPos.y);
    }

    private static void Rotation() {
        
        if (Cam.Main == null) return;
        
        var input = Raylib.GetMouseDelta();

        _rot -= new Vector2(input.Y * Sens, input.X * Sens);
        _rot.X = Math.Clamp(_rot.X, -Clamp, Clamp);

        var forward = new float3(
            
            MathF.Cos(_rot.X) * MathF.Sin(_rot.Y),
            MathF.Sin(_rot.X),
            MathF.Cos(_rot.X) * MathF.Cos(_rot.Y)
        );

        Cam.Main.Target = Cam.Main.Pos + forward;
    }

    private static void Movement() {

        if (Cam.Main == null) return;
        
        var input = float3.zero;

        if (Raylib.IsKeyDown(KeyboardKey.W)) input.z += 1;
        if (Raylib.IsKeyDown(KeyboardKey.S)) input.z -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) input.x += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) input.x -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.E)) input.y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Q)) input.y -= 1;
        
        Cam.Main.Pos += (Cam.Main.Up * input.y + Cam.Main.Right * input.x + Cam.Main.Fwd * input.z) * Speed * Raylib.GetFrameTime();
    }

    private static void set_from_target(float3 pos, float3 target) {
        
        var dir = float3.normalize(target - pos);

        var vertical = MathF.Asin(dir.y);
        var horizontal = MathF.Atan2(dir.x, dir.z);

        _rot = new(vertical, horizontal);
    }
}