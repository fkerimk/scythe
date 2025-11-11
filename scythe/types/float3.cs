using System.Numerics;

namespace scythe;

public struct float3(float X, float Y, float Z) : IEquatable<float3> {
    
    public float x = X;
    public float y = Y;
    public float z = Z;
    
    // operators
    public static bool operator ==(float3 a, float3 b) {
        
        //return a.x == b.x && a.y ==  b.y && a.z == b.z ;
        return Equals(a.x, b.x) && Equals(a.y, b.y) && Equals(a.z, b.z) ;
    }
    
    public static bool operator !=(float3 a, float3 b) {
        
       //return a.x != b.x || a.y !=  b.y || a.z != b.z ;
        return !Equals(a.x, b.x) || !Equals(a.y, b.y) || !Equals(a.z, b.z) ;
    }
    
    public static float3 operator +(float3 a, float3 b) {
        
        return new(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    
    public static float3 operator -(float3 a, float3 b) {
        
        return new(a.x - b.x, a.y - b.y, a.z - b.z);
    }
    
    public static float3 operator -(float3 value) => new(-value.x, -value.y, -value.z);

    public static float3 operator *(float3 a, float b) {
        
        return new(a.x * b, a.y * b, a.z * b);
    }
    
    public static float3 operator *(float3 a, float3 b) {
        
        return new(a.x * b.x, a.y * b.x, a.z * b.x);
    }
    
    public static float3 operator /(float3 a, float b) {
        
        return new(a.x / b, a.y / b, a.z / b);
    }
    
    public static float3 operator /(float3 a, float3 b) {
        
        return new(a.x / b.x, a.y / b.x, a.z / b.x);
    }
    
    // bindings
    public static float3 normalize(float3 value) {

        return Vector3.Normalize(value.to_vector3()).to_float3();
    }
    
    public static float3 cross(float3 a, float3 b) {

        return Vector3.Cross(a.to_vector3(), b.to_vector3()).to_float3();
    }
    
    // pre-values
    public static float3 zero =>  new(0, 0, 0);
    public static float3 one  =>  new(1, 1, 1);
    public static float3 up   =>  new(0, 1, 0);
    
    public bool Equals(float3 other) {
        return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
    }

    public override bool Equals(object? obj) {
        return obj is float3 other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(x, y, z);
    }
}

#pragma warning disable CS8981
public static partial class ext {
    
    public static float3 to_float3(this Vector3 value) {
        
        return new(value.X, value.Y, value.Z);
    }
    
    public static Vector3 to_vector3(this float3 value) {
        
        return new(value.x, value.y, value.z);
    }

    public static float3 to_rad(this float3 value) {
        
        return new(value.x.to_rad(), value.y.to_rad(), value.z.to_rad());
    }
    
    public static float3 to_deg(this float3 value) {
        
        return new(value.x.to_deg(), value.y.to_deg(), value.z.to_deg());
    }
}