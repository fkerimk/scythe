using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public static class freecam {
    
    const float sens = 0.003f;
    const float clamp = 1.5f;
    const float speed = 15;
    
    static Vector2 rot;
    
    public static void init() {
        
        set_from_target(cam.current.Position, cam.current.Target);
    }
    
    public static void update(int2 center) {

        if (Raylib.IsMouseButtonPressed(MouseButton.Right)) {
            
            Raylib.DisableCursor();
        }

        if (Raylib.IsMouseButtonReleased(MouseButton.Right)) {
            
            Raylib.EnableCursor();
            Raylib.SetMousePosition(center.x, center.y);
        }
        
        if (!Raylib.IsMouseButtonDown(MouseButton.Right)) return;

        movement();
        rotation();
    }

    private static void rotation() {
        
        var input = Raylib.GetMouseDelta();

        rot -= new Vector2(input.Y * sens, input.X * sens);
        rot.X = Math.Clamp(rot.X, -clamp, clamp);

        var forward = new Vector3(
            
            MathF.Cos(rot.X) * MathF.Sin(rot.Y),
            MathF.Sin(rot.X),
            MathF.Cos(rot.X) * MathF.Cos(rot.Y)
        );

        cam.current.Target = cam.current.Position + forward;
    }

    private static void movement() {

        var input = Vector3.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.W)) input.Z += 1;
        if (Raylib.IsKeyDown(KeyboardKey.S)) input.Z -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) input.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) input.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.E)) input.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Q)) input.Y -= 1;
        
        var fwd = Vector3.Normalize(cam.current.Target - cam.current.Position);
        var right = Vector3.Normalize(Vector3.Cross(fwd, cam.current.Up));
        var up = -Vector3.Normalize(Vector3.Cross(fwd, right));
        
        cam.current.Position += (up * input.Y + right * input.X + fwd * input.Z) * speed * Raylib.GetFrameTime();
    }

    private static void set_from_target(Vector3 pos, Vector3 target) {
        
        var dir = Vector3.Normalize(target - pos);

        var vertical = MathF.Asin(dir.Y);
        var horizontal = MathF.Atan2(dir.X, dir.Z);

        rot = new(vertical, horizontal);
    }
}