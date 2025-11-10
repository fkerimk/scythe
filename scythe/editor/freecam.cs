using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public class freecam {

    private readonly cam cam;
    
    const float sens = 0.003f;
    const float clamp = 1.55f;
    const float speed = 15;
    
    private Vector2 rot;
    
    private bool is_locked;
    private int2 lock_pos;

    public freecam(cam cam) {
        
        this.cam = cam;

        set_from_target(cam.pos, cam.target);
    }
    
    public void loop(viewport viewport) {

        var center = viewport.window_pos.to_int2() + viewport.content_region.to_int2() / 2;
        
        if (viewport.isHovered && Raylib.IsMouseButtonPressed(MouseButton.Right)) {

            is_locked = true;
            //lock_pos = Raylib.GetMousePosition();
            lock_pos = center;
            Raylib.DisableCursor();
        }

        if (is_locked && Raylib.IsMouseButtonReleased(MouseButton.Right)) {
            
            Raylib.EnableCursor();
            Raylib.SetMousePosition(lock_pos.x, lock_pos.y);
            
            is_locked = false;
        }
        
        if (!is_locked) return;
        
        movement();
        rotation();
        
        Raylib.SetMousePosition(lock_pos.x, lock_pos.y);
    }

    private void rotation() {
        
        var input = Raylib.GetMouseDelta();

        rot -= new Vector2(input.Y * sens, input.X * sens);
        rot.X = Math.Clamp(rot.X, -clamp, clamp);

        var forward = new float3(
            
            MathF.Cos(rot.X) * MathF.Sin(rot.Y),
            MathF.Sin(rot.X),
            MathF.Cos(rot.X) * MathF.Cos(rot.Y)
        );

        cam.target = cam.pos + forward;
    }

    private void movement() {

        var input = float3.zero;

        if (Raylib.IsKeyDown(KeyboardKey.W)) input.z += 1;
        if (Raylib.IsKeyDown(KeyboardKey.S)) input.z -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) input.x += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) input.x -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.E)) input.y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Q)) input.y -= 1;
        
        cam.pos += (cam.up * input.y + cam.right * input.x + cam.fwd * input.z) * speed * Raylib.GetFrameTime();
    }

    private void set_from_target(float3 pos, float3 target) {
        
        var dir = float3.normalize(target - pos);

        var vertical = MathF.Asin(dir.y);
        var horizontal = MathF.Atan2(dir.x, dir.z);

        rot = new(vertical, horizontal);
    }
}