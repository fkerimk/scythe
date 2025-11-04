using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public static class cam {

    public static Camera3D current;
    
    public static void init() {
        
        current = new() {
            
            Projection = CameraProjection.Perspective,
            FovY = 90,
            Position = new(6, 6, 6),
            Up = new(0, 1, 0)
        };
    }
}