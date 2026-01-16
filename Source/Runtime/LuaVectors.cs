using System.Numerics;
using Raylib_cs;

public class LuaF2 {
     
     public static Func<float, float, Vector2> New => (x, y) => new Vector2(x, y);
     public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => Raymath.Vector2Lerp(a, b, t);
     public static Vector2 Zero => Vector2.Zero;
     public static Vector2 Up => Vector2.UnitY;
     public static Vector2 Down => -Vector2.UnitY;
     public static Vector2 Right => Vector2.UnitX;
     public static Vector2 Left => -Vector2.UnitX;
}


public class LuaF3 {
     
     public static Vector3 Zero => Vector3.Zero;
     public static Vector3 Up => Vector3.UnitY;
     public static Vector3 Down => -Vector3.UnitY;
     public static Vector3 Fwd => Vector3.UnitZ;
     public static Vector3 Back => -Vector3.UnitZ;
     public static Vector3 Right => Vector3.UnitX;
     public static Vector3 Left => -Vector3.UnitX;
     
     public static Func<float, float, float, Vector3> New => (x, y, z) => new Vector3(x, y, z);
     
     public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => Raymath.Vector3Lerp(a, b, t);

     public static Vector3 FromQuaternion(Quaternion q) {

          var mat = Matrix4x4.CreateFromQuaternion(q);
          return new Vector3(mat.M31, mat.M32, mat.M33);
     }
     
     public static Vector3 Normalize(Vector3 value) => Vector3.Normalize(value);
}