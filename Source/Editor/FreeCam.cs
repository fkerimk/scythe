using System.Numerics;
using Raylib_cs;

internal class FreeCam {
    
    private const float Sens = 0.003f;
    private const float Clamp = 1.55f;
    private const float Speed = 15;
    
    private Vector3 _pos;
    private Vector3 _lerpedPos;
    private Vector3 _forward;
    
    private Vector2 _rot;
    
    private bool _isLocked;
    private Vector2 _lockPos;

    private readonly Camera3D _camera;

    public FreeCam (Camera3D camera) {

        _camera = camera;
        
        SetFromTarget(_camera.Position, _camera.Target);
    }
    
    public void Loop(Viewport viewport) {
        
        var center = viewport.WindowPos + viewport.ContentRegion / 2;
        
        if (viewport.IsHovered && Raylib.IsMouseButtonPressed(MouseButton.Right)) {

            _isLocked = true;
            _lockPos = center;
            
            Raylib.DisableCursor();
        }

        if (_isLocked && Raylib.IsMouseButtonReleased(MouseButton.Right)) {
            
            Raylib.EnableCursor();
            Raylib.SetMousePosition((int)_lockPos.X, (int)_lockPos.Y);
            
            _isLocked = false;
        }
        
        
        _lerpedPos = Raymath.Vector3Lerp(_lerpedPos, _pos, Raylib.GetFrameTime() * 15);
        
        _camera.Position = _lerpedPos;
        _camera.Target = _lerpedPos + _forward;
        
        if (!_isLocked) return;
        
        Movement();
        Rotation();
        
        Raylib.SetMousePosition((int)_lockPos.X, (int)_lockPos.Y);
    }

    private void Rotation() {
        
        var input = Raylib.GetMouseDelta();

        _rot -= new Vector2(input.Y * Sens, input.X * Sens);
        _rot.X = Math.Clamp(_rot.X, -Clamp, Clamp);

        _forward = new Vector3(
            
            MathF.Cos(_rot.X) * MathF.Sin(_rot.Y),
            MathF.Sin(_rot.X),
            MathF.Cos(_rot.X) * MathF.Cos(_rot.Y)
        );
    }

    private void Movement() {

        var input = Vector3.Zero;

        if (Raylib.IsKeyDown(KeyboardKey.W)) input.Z += 1;
        if (Raylib.IsKeyDown(KeyboardKey.S)) input.Z -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.D)) input.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.A)) input.X -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.E)) input.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.Q)) input.Y -= 1;
        
        _pos += (_camera.Up * input.Y + _camera.Right * input.X + _camera.Fwd * input.Z) * Speed * Raylib.GetFrameTime();
    }

    private void SetFromTarget(Vector3 pos, Vector3 target) {
        
        var dir = Raymath.Vector3Normalize(target - pos);

        var vertical = MathF.Asin(dir.Y);
        var horizontal = MathF.Atan2(dir.X, dir.Z);

        _lerpedPos = _pos = _camera.Position;
        _rot = new Vector2(vertical, horizontal);
        
        _forward = new Vector3(
            
            MathF.Cos(_rot.X) * MathF.Sin(_rot.Y),
            MathF.Sin(_rot.X),
            MathF.Cos(_rot.X) * MathF.Cos(_rot.Y)
        );
    }
}