using System.Numerics;
using Raylib_cs;

internal class LuaMt {

    public static float Lerp(float a, float b, float t) => Raymath.Lerp(a, b, t);
    public static float Clamp(float value, float min, float max) => Raymath.Clamp(value, min, max);

    public static float DirAngle(Vector2 dir) {

        var rad = Math.Atan2(dir.Y, dir.X);
        var deg = rad * (180f / Math.PI);
        var angle = (deg + 360) % 360;

        return (float)angle;
    }

    public static float Sign(float value) => MathF.Sign(value);
}