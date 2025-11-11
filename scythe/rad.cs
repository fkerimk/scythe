namespace scythe;

#pragma warning disable CS8981
public static partial class ext {
    
    private const float deg2rad = MathF.PI / 180.0f;
    private const float rad2deg = 180.0f / MathF.PI;

    public static float to_rad(this float degrees) {
        
        return degrees * deg2rad;
    }

    public static float to_deg(this float radians) {
        
        return radians * rad2deg;
    }
}