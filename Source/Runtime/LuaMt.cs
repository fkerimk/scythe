using System.Numerics;
using Raylib_cs;

internal class LuaMt {

    public static float Clamp(float value, float min, float max) => Raymath.Clamp(value, min, max);
    public static Quaternion RotDir(Vector3 dir) => Raymath.QuaternionFromVector3ToVector3(Vector3.UnitZ, dir);
    public static Quaternion Multiply(Quaternion a, Quaternion b) => Raymath.QuaternionMultiply(a, b);
    public static Quaternion RotFromAxisAngle(Vector3 axis, float angle) => Raymath.QuaternionFromAxisAngle(axis, angle * Raylib.DEG2RAD);
}