using System.Numerics;

public struct float4(float x, float y, float z, float w) : IEquatable<float4> {
    
    // values
    public float x = x;
    public float y = y;
    public float z = z;
    public float w = w;
    
    // operators
    public static float4 operator -(float4 value) => value * -1;
    public static float4 operator -(float4 a, float4 b) { return new(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w); }
    public static float4 operator +(float4 a, float4 b) { return new(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w); }
    public static float4 operator *(float4 a, float4 b) { return new(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w); }
    public static float4 operator *(float4 a, float  b) { return new(a.x * b  , a.y * b  , a.z * b  , a.w * b  ); }
    public static float4 operator /(float4 a, float4 b) { return new(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w); }
    public static float4 operator /(float4 a, float  b) { return new(a.x / b  , a.y / b  , a.z / b  , a.w / b  ); }
    
    // bindings
    public static float4 normalize(float4 value) {  return Vector4.Normalize(value.to_vector4()).to_float4(); }
    public static float4 cross(float4 a, float4 b) { return Vector4.Cross(a.to_vector4(), b.to_vector4()).to_float4(); }
    
    // pre-values
    public static float4 zero  =>  new(0, 0, 0, 0);
    public static float4 right =>  new(1, 0, 0, 0);
    public static float4 up    =>  new(0, 1, 0, 0);
    public static float4 fwd   =>  new(0, 0, 1, 0);
    public static float4 forth =>  new(0, 0, 0, 1);
    public static float4 one   =>  new(1, 1, 1, 1);
    
    // equatable
    public bool Equals(float4 other) { return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w); }
    public override bool Equals(object? obj) { return obj is float4 other && Equals(other); }
    public static bool operator ==(float4 a, float4 b) { return  Equals(a, b); }
    public static bool operator !=(float4 a, float4 b) { return !Equals(a, b); }
    
    // overrides
    public override int GetHashCode() { return HashCode.Combine(x, y, z); }
    public override string ToString() => $"{x}, {y}, {z}";
}

internal static partial class Extensions {
    
    // conversions
    public static float4  to_float4(this Vector4 value) { return new(value.X, value.Y, value.Z, value.W); }
    public static Vector4 to_vector4(this float4 value) { return new(value.x, value.y, value.z, value.w); }
    public static float4 to_rad(this float4 value) { return new(value.x.DegToRad(), value.y.DegToRad(), value.z.DegToRad(), value.w.DegToRad()); }
    public static float4 to_deg(this float4 value) { return new(value.x.RadToDeg(), value.y.RadToDeg(), value.z.RadToDeg(), value.w.RadToDeg()); }
}