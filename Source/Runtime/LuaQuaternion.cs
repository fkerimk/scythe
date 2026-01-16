using System.Numerics;

internal class LuaQuat {

    public static Quaternion Identity => Quaternion.Identity;
    
    public static Quaternion FromEuler(float x, float y, float z) => Quaternion.CreateFromYawPitchRoll(y * (MathF.PI / 180f), x * (MathF.PI / 180f), z * (MathF.PI / 180f));
    public static Quaternion FromEuler(Vector3 euler) => FromEuler(euler.X, euler.Y, euler.Z);
    
    public static Quaternion Multiply(Quaternion a, Quaternion b) => a * b;
    
    public static Quaternion Lerp (Quaternion a, Quaternion b, float t) => Quaternion.Lerp(a, b, t);
}