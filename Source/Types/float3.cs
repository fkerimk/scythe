using System.Numerics;

public struct float3(float x, float y, float z) : IEquatable<float3> {
    
    // values
    public float x = x;
    public float y = y;
    public float z = z;
    
    // operators
    public static float3 operator -(float3 value) => value * -1;
    public static float3 operator -(float3 a, float3 b) { return new(a.x - b.x, a.y - b.y, a.z - b.z); }
    public static float3 operator +(float3 a, float3 b) { return new(a.x + b.x, a.y + b.y, a.z + b.z); }
    public static float3 operator *(float3 a, float3 b) { return new(a.x * b.x, a.y * b.y, a.z * b.z); }
    public static float3 operator *(float3 a, float  b) { return new(a.x * b  , a.y * b  , a.z * b  ); }
    public static float3 operator /(float3 a, float3 b) { return new(a.x / b.x, a.y / b.y, a.z / b.z); }
    public static float3 operator /(float3 a, float  b) { return new(a.x / b  , a.y / b  , a.z / b  ); }
    
    // bindings
    public static float3 normalize(float3 value) {  return Vector3.Normalize(value.to_vector3()).to_float3(); }
    public static float3 cross(float3 a, float3 b) { return Vector3.Cross(a.to_vector3(), b.to_vector3()).to_float3(); }
    
    // pre-values
    public static float3 zero  =>  new(0, 0, 0);
    public static float3 right =>  new(1, 0, 0);
    public static float3 up    =>  new(0, 1, 0);
    public static float3 fwd   =>  new(0, 0, 1);
    public static float3 one   =>  new(1, 1, 1);
    
    // equatable
    public bool Equals(float3 other) { return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z); }
    public override bool Equals(object? obj) { return obj is float3 other && Equals(other); }
    public static bool operator ==(float3 a, float3 b) { return  Equals(a, b); }
    public static bool operator !=(float3 a, float3 b) { return !Equals(a, b); }
    
    // overrides
    public override int GetHashCode() { return HashCode.Combine(x, y, z); }
    public override string ToString() => $"{x}, {y}, {z}";
}

internal static partial class Extensions {
    
    // conversions
    public static float3  to_float3(this Vector3 value) { return new(value.X, value.Y, value.Z); }
    public static Vector3 to_vector3(this float3 value) { return new(value.x, value.y, value.z); }
    public static float3 to_rad(this float3 value) { return new(value.x.DegToRad(), value.y.DegToRad(), value.z.DegToRad()); }
    public static float3 to_deg(this float3 value) { return new(value.x.RadToDeg(), value.y.RadToDeg(), value.z.RadToDeg()); }
}