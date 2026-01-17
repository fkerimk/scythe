using System.Numerics;
using Raylib_cs;

internal class Camera3D {

    public Raylib_cs.Camera3D Raylib = new() {
                
        Projection = CameraProjection.Perspective,
        FovY = 60,
        Position = new Vector3(2, 2, 2),
        Target = new Vector3(0, 1, 0),
        Up = new Vector3(0, 1, 0)
    };
    
    public CameraProjection Projection { get => Raylib.Projection; set => Raylib.Projection = value; }
    public float FovY { get => Raylib.FovY; set => Raylib.FovY = value; }
    public Vector3 Position { get => Raylib.Position; set => Raylib.Position = value; }
    public Vector3 Target { get => Raylib.Target; set => Raylib.Target = value; }
    public Vector3 Up { get => Raylib.Up; set => Raylib.Up = value; }
}

internal static partial class Extensions {
    
    extension(Camera3D camera) {
        
        public void DrawCameraFrustum(Raylib_cs.Color color) {
        
            const float near = 0.1f, far = 10.0f;

            const int fov = 60;
        
            var aspect = (float)Raylib.GetScreenWidth() / Raylib.GetScreenHeight();
            var topNear = MathF.Tan(fov * Raylib.DEG2RAD / 2.0f) * near;
            var rightNear = topNear * aspect;
            var topFar = MathF.Tan(fov * Raylib.DEG2RAD / 2.0f) * far;
            var rightFar = topFar * aspect;

            var forward = Vector3.Normalize(camera.Target - camera.Position);
            var right = Vector3.Normalize(Vector3.Cross(forward, camera.Up));
            var up = Vector3.Cross(right, forward);

            var nearCenter = camera.Position + forward * near;
            var corners = new Vector3[8];
        
            corners[0] = nearCenter + (up * topNear) - (right * rightNear);
            corners[1] = nearCenter + (up * topNear) + (right * rightNear);
            corners[2] = nearCenter - (up * topNear) + (right * rightNear);
            corners[3] = nearCenter - (up * topNear) - (right * rightNear);

            var farCenter = camera.Position + forward * far;
            corners[4] = farCenter + (up * topFar) - (right * rightFar);
            corners[5] = farCenter + (up * topFar) + (right * rightFar);
            corners[6] = farCenter - (up * topFar) + (right * rightFar);
            corners[7] = farCenter - (up * topFar) - (right * rightFar);

            for (var i = 0; i < 4; i++) {
            
                Raylib.DrawLine3D(corners[i], corners[(i + 1) % 4], color);
                Raylib.DrawLine3D(corners[i + 4], corners[((i + 1) % 4) + 4], color);
                Raylib.DrawLine3D(corners[i], corners[i + 4], color);
            }
        }

        public Vector3 Up => Raymath.Vector3Normalize(Raymath.Vector3CrossProduct(camera.Fwd, camera.Right));
        public Vector3 Fwd => Raymath.Vector3Normalize(camera.Target - camera.Position);
        public Vector3 Right => Raymath.Vector3Normalize(Raymath.Vector3CrossProduct(camera.Fwd, Vector3.UnitY));
    }
}