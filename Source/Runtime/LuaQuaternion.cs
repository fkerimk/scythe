using System.Numerics;

internal class LuaQuat {

    public static Quaternion Identity => Quaternion.Identity;

    public static Quaternion FromEuler(float x, float y, float z) => Quaternion.CreateFromYawPitchRoll(y * (MathF.PI / 180f), x * (MathF.PI / 180f), z * (MathF.PI / 180f));

    public static Quaternion Multiply(Quaternion a, Quaternion b) => a * b;

    public static Quaternion Lerp(Quaternion a, Quaternion b, float t) => Quaternion.Lerp(a, b, t);

    public static Quaternion FromDir(Vector3 dir) {

        var forward = Vector3.Normalize(dir);

        if (forward.LengthSquared() < 0.000001f) return Quaternion.Identity;

        var up    = Vector3.UnitY;
        var right = Vector3.Cross(up, forward);

        if (right.LengthSquared() < 0.000001f) right = Vector3.Cross(Vector3.UnitZ, forward);

        right = Vector3.Normalize(right);
        up    = Vector3.Cross(forward, right);

        var mat = new Matrix4x4(right.X, right.Y, right.Z, 0, up.X, up.Y, up.Z, 0, forward.X, forward.Y, forward.Z, 0, 0, 0, 0, 1);

        return Quaternion.CreateFromRotationMatrix(mat);
    }
}