using System.Numerics;

public struct int2(int x, int y) : IEquatable<int2> {
    
    // values
    public int x = x;
    public int y = y;
    
    // operators
    public static int2 operator -(int2 value) => value * -1;
    public static int2 operator -(int2 a, int2  b) { return new(      a.x - b.x,       a.y - b.y); }
    public static int2 operator +(int2 a, int2  b) { return new(      a.x + b.x,       a.y + b.y); }
    public static int2 operator *(int2 a, int2  b) { return new(      a.x * b.x,       a.y * b.y); }
    public static int2 operator *(int2 a, float b) { return new((int)(a.x * b ), (int)(a.y * b )); }
    public static int2 operator /(int2 a, int2  b) { return new(      a.x / b.x,       a.y / b.y); }
    public static int2 operator /(int2 a, float b) { return new((int)(a.x / b ), (int)(a.y / b )); }
    
    // bindings
    public static int2 normalize(int2 value) { return Vector2.Normalize(value.to_vector2()).to_int2(); }
    public static float cross(int2 a, int2 b) { return Vector2.Cross(a.to_vector2(), b.to_vector2()); }
    
    // pre-values
    public static int2 zero  =>  new(0, 0);
    public static int2 right =>  new(1, 0);
    public static int2 up    =>  new(0, 1);
    public static int2 one   =>  new(1, 1);
    
    // equatable
    public bool Equals(int2 other) { return x.Equals(other.x) && y.Equals(other.y); }
    public override bool Equals(object? obj) { return obj is int2 other && Equals(other); }
    public static bool operator ==(int2 a, int2 b) { return  Equals(a, b); }
    public static bool operator !=(int2 a, int2 b) { return !Equals(a, b); }
    
    // overrides
    public override int GetHashCode() { return HashCode.Combine(x, y); }
    public override string ToString() => $"{x}, {y}";
    
    // constructors
    public int2(float x, float y) : this((int)x, (int)y) { }
}

internal static partial class Extensions {
    
    // conversions
    public static int2    to_int2(this Vector2 value) { return new((int)value.X, (int)value.Y); }
    public static Vector2 to_vector2(this int2 value) { return new(value.x, value.y); }
}