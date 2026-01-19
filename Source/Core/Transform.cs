using System.Numerics;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Raylib_cs;
using static Raylib_cs.Raylib;

[MoonSharpUserData]
[JsonObject(MemberSerialization.OptIn)]
internal class Transform(Obj obj) : Component(obj) {
    
    public override string LabelIcon => Icons.Transform;
    public override Color LabelColor => Colors.GuiTypeTransform;

    #region Local Transform

    [Label("Pos"), RecordHistory, JsonProperty]
    public new Vector3 Pos { get; set { field = value; UpdateTransform(); } } = Vector3.Zero;

    [Label("Euler"), RecordHistory, JsonProperty]
    public Vector3 Euler { 
        
        get {
            
            var e = Raymath.QuaternionToEuler(Rot).ToDeg();
            return new Vector3(e.X, -e.Y, -e.Z);
            
        } set {
            
            var q = Raymath.QuaternionFromEuler(value.X.DegToRad(), (-value.Y).DegToRad(), (-value.Z).DegToRad());
            if (MathF.Abs(Quaternion.Dot(Rot, q)) > 0.9999f) return;
            Rot = q;
        }
    }

    [Label("Scale"), RecordHistory, JsonProperty]
    public Vector3 Scale { get; set { field = value; UpdateTransform(); } } = Vector3.One;

    [RecordHistory, JsonProperty]
    public new Quaternion Rot { get; set { field = value; UpdateTransform(); } } = Quaternion.Identity;

    #endregion
    
    #region World Transform
    public Vector3 WorldPos {

        get {

            Obj.DecomposeWorldMatrix(out var trans, out _, out _);
            return trans;
            
        } set {

            if (Obj.Parent == null) Pos = value; else {

                var invParent = Raymath.MatrixInvert(Obj.Parent.WorldMatrix);
                Pos = Raymath.Vector3Transform(value, invParent);
            }
        }
    }

    public Quaternion WorldRot {

        get {

            Obj.DecomposeWorldMatrix(out _, out var rot, out _);
            return rot;
            
        } set {

            if (Obj.Parent == null) Rot = value; else {

                var parentRot = Quaternion.CreateFromRotationMatrix(Obj.Parent.WorldRotMatrix);
                var q = Quaternion.Inverse(parentRot) * value;
                if (MathF.Abs(Quaternion.Dot(Rot, q)) > 0.9999f) return;
                Rot = q;
            }
        }
    }

    public Vector3 WorldEuler { 
        
        get {
            
            var e = Raymath.QuaternionToEuler(WorldRot).ToDeg();
            return new Vector3(e.X, -e.Y, -e.Z);
            
        } set {
            
            var q = Raymath.QuaternionFromEuler(value.X.DegToRad(), (-value.Y).DegToRad(), (-value.Z).DegToRad());
            if (MathF.Abs(Quaternion.Dot(WorldRot, q)) > 0.9999f) return;
            WorldRot = q;
        }
    }
    #endregion

    private const float MoveSnap = 0.2f;
    
    private int _mode;
    private float _activeMove;
    private string _activeId = "";

    private bool
        _isWorldSpace,
        _canUseShortcuts;
    
    private Vector3
        _activePos,
        _activeWorldPos,
        _activeLocalAxis,
        _activeScale,
        _activeNormal,
        _activeInitialPoint,
        _activeInitialVector;
    
    private Vector2 _activeMouseTemp;
    private Quaternion _activeRot = Quaternion.Identity;

    public void UpdateTransform() {
        
        var rotMatrix = Matrix4x4.Transpose(Matrix4x4.CreateFromQuaternion(Rot));
        
        var matrix = Raymath.MatrixMultiply(
            
            Raymath.MatrixMultiply(
                
                Raymath.MatrixScale(Scale.X, Scale.Y, Scale.Z),
                rotMatrix
            ),
            
            Raymath.MatrixTranslate(Pos.X, Pos.Y, Pos.Z)
        );
    
        Obj.RotMatrix = rotMatrix;
        Obj.Matrix = matrix;

        RefreshWorldMatrices(Obj);
    }

    private static void RefreshWorldMatrices(Obj obj) {
        
        if (obj.Parent != null) {
            
            obj.WorldMatrix = obj.Parent.WorldMatrix * obj.Matrix;
            obj.WorldRotMatrix = obj.Parent.WorldRotMatrix * obj.RotMatrix;
            
        } else {
            
            obj.WorldMatrix = obj.Matrix;
            obj.WorldRotMatrix = obj.RotMatrix;
        }

        foreach (var child in obj.Children)
            RefreshWorldMatrices(child.Value);
    }

    public override void Loop(bool is2D) {

        if (is2D) {
            
            _canUseShortcuts = false;
        
            if (IsCursorHidden()) return;
        
            if (!Obj.IsSelected) return;

            _canUseShortcuts = true;
        
            var modeName = _mode switch { 0 => "Pos", 1 => "Rot", 2 => "Scale", _ => "Bruh" };
            var spaceName = (_mode == 2) ? ("") : (_isWorldSpace ? "(World)" : "(Local)");

            var textA = $"{modeName} {spaceName}";
            var textPosA = Editor.Level3D.RelativeMouse with { Y = Editor.Level3D.RelativeMouse.Y - 15 };
        
            DrawText(textA, (int)textPosA.X - 14, (int)textPosA.Y - 19, 20, Color.Black);
            DrawText(textA, (int)textPosA.X - 15, (int)textPosA.Y - 20, 20, Color.Yellow);
        
            if (_activeMove == 0) return;
        
            var textB = _mode switch { 0 or 2 => $"{_activeMove:F2}m", 1 => $"{_activeMove:F2}°", _ => $"{_activeMove:F2}" }; 
            var textPosB = Editor.Level3D.RelativeMouse with { Y = Editor.Level3D.RelativeMouse.Y - 15 };
            
            DrawText(textB, (int)textPosB.X - 14, (int)textPosB.Y - 39, 20, Color.Black);
            DrawText(textB, (int)textPosB.X - 15, (int)textPosB.Y - 40, 20, Color.Yellow);
            
        } else {
        
            UpdateTransform();
            
            if (!CommandLine.Editor || Core.ActiveCamera == null || !Core.IsRendering) return;

            if (!Obj.IsSelected) return;
            
            if (_canUseShortcuts && _activeMove == 0) {
            
                if (IsKeyPressed(KeyboardKey.Q)) _mode = 0;
                if (IsKeyPressed(KeyboardKey.W)) _mode = 1;
                if (IsKeyPressed(KeyboardKey.E)) _mode = 2;
                if (IsKeyPressed(KeyboardKey.X)) _isWorldSpace = !_isWorldSpace;
            }
            
            BeginShaderMode(Shaders.Transform);

            var ray = GetScreenToWorldRay(Level3D.RelativeMouse3D, Core.ActiveCamera.Raylib);

            var useWorld = _isWorldSpace && _mode != 2;

            var r = useWorld ? Vector3.UnitX : Obj.Right;
            var u = useWorld ? Vector3.UnitY : Obj.Up;
            var f = useWorld ? Vector3.UnitZ : Obj.Fwd;
            
            Axis("x", Vector3.UnitX, r, new Color(0.9f, 0.3f, 0.3f), ray);
            Axis("y", Vector3.UnitY, u, new Color(0.3f, 0.9f, 0.3f), ray);
            Axis("z", Vector3.UnitZ, f, new Color(0.3f, 0.3f, 0.9f), ray);
            
            EndShaderMode();
        }
    }

    private void Axis(string id, Vector3 axis, Vector3 normal, Color color, Ray ray) {

        if (Core.ActiveCamera == null) return;
        
        var isActive = _activeId == id;
        
        Obj.DecomposeWorldMatrix(out var worldPos, out _, out _);
        
        var a = worldPos + (Raymath.Vector3Normalize(normal) * 0.1f);
        var b = a + normal * 1.5f;

        if (!string.IsNullOrEmpty(_activeId) && _activeId != id) {
            
            DrawLine3D(a, b, color);
            return;
        }

        if (isActive) {

            var newPos = Pos;
            var newRot = Rot;
            var newScale = Scale;

            switch (_mode) {
                
                case 0: { // Position
                    
                    var currentPoint = GetClosestPointLineRay(_activeWorldPos, _activeNormal, ray);
                    var delta = currentPoint - _activeInitialPoint;
                    
                    if (!IsKeyDown(KeyboardKey.LeftShift)) {
                        
                        delta.X = MathF.Round(delta.X / MoveSnap) * MoveSnap;
                        delta.Y = MathF.Round(delta.Y / MoveSnap) * MoveSnap;
                        delta.Z = MathF.Round(delta.Z / MoveSnap) * MoveSnap;
                    }

                    var newWorldPos = _activeWorldPos - delta;

                    if (Obj.Parent != null) {

                        var inverseParentMatrix = Raymath.MatrixInvert(Obj.Parent.WorldMatrix);
                        newPos = Raymath.Vector3Transform(newWorldPos, inverseParentMatrix);
                    }
                    
                    else newPos = newWorldPos;
                    
                    _activeMove = Vector3.Dot(delta, _activeNormal);
                    break;
                }

                
                case 1: { // Rotation
                    
                    if (IntersectRayPlane(ray, worldPos, _activeNormal, out var currentPoint)) {
                        
                        var currentVector = Vector3.Normalize(currentPoint - _activeWorldPos);
                        var angleRad = MathF.Atan2(Vector3.Dot(_activeNormal, Vector3.Cross(_activeInitialVector, currentVector)), Vector3.Dot(_activeInitialVector, currentVector));
                        var angleDeg = angleRad * (180f / MathF.PI);

                        if (!IsKeyDown(KeyboardKey.LeftShift)) angleDeg = MathF.Round(angleDeg / 22.5f) * 22.5f;
                        _activeMove = angleDeg;

                        var deltaRot = Quaternion.CreateFromAxisAngle(_activeLocalAxis, _activeMove.DegToRad());

                        if (_isWorldSpace) {

                            if (Obj.Parent != null) {

                                var parentRot = Quaternion.CreateFromRotationMatrix(Obj.Parent.WorldRotMatrix);
                                var localDeltaInParentSpace = Quaternion.Inverse(parentRot) * deltaRot * parentRot;

                                newRot = localDeltaInParentSpace * _activeRot;
                            }

                            else newRot = deltaRot * _activeRot;
                        }

                        else newRot = _activeRot * deltaRot;

                        // Preserve hemisphere to prevent sudden Euler flips
                        if (Quaternion.Dot(newRot, _activeRot) < 0) newRot = Quaternion.Negate(newRot);
                    }
                    
                    break;
                }

                case 2: { // Scale
                
                    var currentPoint = GetClosestPointLineRay(_activeWorldPos, _activeNormal, ray);
                    var delta = currentPoint - _activeInitialPoint;
                    var move = -Vector3.Dot(delta, _activeNormal);

                    if (!IsKeyDown(KeyboardKey.LeftShift)) move = MathF.Round(move / MoveSnap) * MoveSnap;
                    _activeMove = move;

                    newScale = _activeScale;
                    
                    switch (id) {
                        
                        case "x": newScale.X += move; break;
                        case "y": newScale.Y += move; break;
                        case "z": newScale.Z += move; break;
                    }
                    
                    break;
                }
            }
            
            Pos = newPos;
            Rot = newRot;
            Scale = newScale;
        }
        
        const float
            radius = 0.025f,
            rayRadius = 0.125f;
        
        const int rayQuality = 64;
        
        var isHovered = false;
        var centerPos = worldPos;

        if (_mode == 1) { // Rotation circle collision
            
            var normal1 = Vector3.Normalize(Vector3.Cross(normal, MathF.Abs(normal.Y) > 0.9f ? Vector3.UnitX : Vector3.UnitY));
            var normal2 = Vector3.Cross(normal, normal1);

            for (var i = 0; i < rayQuality + 1; i++) {
                
                var angle = (i / (float)rayQuality) * MathF.PI * 2f;
                var step = centerPos + (normal1 * MathF.Cos(angle) + normal2 * MathF.Sin(angle)) * 1.5f;
                
                if (GetRayCollisionSphere(ray, step, rayRadius * 1.5f).Hit) isHovered = true;
            }
            
        } else { // Line collision
            
            for (var i = 0; i < rayQuality + 1; i++) {
                
                var step = Vector3.Lerp(a, b, 1f / rayQuality * i);
                
                if (GetRayCollisionSphere(ray, step, rayRadius).Hit) isHovered = true;
            }
        }
        
        if (isHovered && IsMouseButtonPressed(MouseButton.Left)) {
            
            _activeId = id;
            _activeLocalAxis = axis;
            _activePos = Pos;
            _activeRot = Rot;
            _activeScale = Scale;
    
            _activeMouseTemp = GetMousePosition();
            _activeNormal = normal;

            _activeWorldPos = worldPos;
            
            if (_mode == 1 && IntersectRayPlane(ray, _activeWorldPos, _activeNormal, out _activeInitialPoint))
                _activeInitialVector = Vector3.Normalize(_activeInitialPoint - _activeWorldPos);
            
            else _activeInitialPoint = GetClosestPointLineRay(_activeWorldPos, _activeNormal, ray);
            
            History.StartRecording(this, "Transform");
        }

        if ((isActive && IsMouseButtonReleased(MouseButton.Left)) || IsCursorHidden()) {
            
            _activeId = "";
            _activeMove = 0;
            
            History.StopRecording();
        }

        var targetColor = (!isActive && isHovered && !IsCursorHidden()) ? Color.White : color;
        
        if (_mode == 1) { // Draw rotation circle
            
            var normal1 = Vector3.Normalize(Vector3.Cross(normal, MathF.Abs(normal.Y) > 0.9f ? Vector3.UnitX : Vector3.UnitY));
            var normal2 = Vector3.Cross(normal, normal1);
            
            for (var i = 0; i < 48; i++) {
                
                var angle1 = (i / 48f) * MathF.PI * 2f;
                var angle2 = ((i + 1) / 48f) * MathF.PI * 2f;
                var p1 = centerPos + (normal1 * MathF.Cos(angle1) + normal2 * MathF.Sin(angle1)) * 1.5f;
                var p2 = centerPos + (normal1 * MathF.Cos(angle2) + normal2 * MathF.Sin(angle2)) * 1.5f;
                
                DrawCylinderEx(p1, p2, radius, radius, 1, targetColor);
            }
        }
        
        else DrawCylinderEx(a, b, radius, radius, 1, targetColor);
    }
    
    public void RotateX(float deg) => Rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, deg.DegToRad()) * Rot;

    public void RotateY(float deg) => Rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (-deg).DegToRad()) * Rot;

    public void RotateZ(float deg) => Rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (-deg).DegToRad()) * Rot;

    public void AddEuler(float x, float y, float z) {
        
        var q = Quaternion.CreateFromYawPitchRoll((-y).DegToRad(), x.DegToRad(), (-z).DegToRad());
        Rot = q * Rot;
    }

    private static Vector3 GetClosestPointLineRay(Vector3 lineStart, Vector3 lineDir, Ray ray) {
        
        var w0 = ray.Position - lineStart;
        var a = Vector3.Dot(lineDir, lineDir);
        var b = Vector3.Dot(lineDir, ray.Direction);
        var c = Vector3.Dot(ray.Direction, ray.Direction);
        var d = Vector3.Dot(lineDir, w0);
        var e = Vector3.Dot(ray.Direction, w0);
        var denominator = a * c - b * b;
        if (MathF.Abs(denominator) < 1e-6f) return lineStart;
        var s = (b * e - c * d) / denominator;
        return lineStart + lineDir * s;
    }

    private static bool IntersectRayPlane(Ray ray, Vector3 planePos, Vector3 planeNormal, out Vector3 intersection) {
        
        var denominator = Vector3.Dot(planeNormal, ray.Direction);
        
        if (MathF.Abs(denominator) > 1e-6f) {
            
            var t = Vector3.Dot(planePos - ray.Position, planeNormal) / denominator;
            
            if (t >= 0) {
                
                intersection = ray.Position + ray.Direction * t;
                return true;
            }
        }
        
        intersection = Vector3.Zero;
        return false;
    }
}