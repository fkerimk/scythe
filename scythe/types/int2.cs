using System.Numerics;

namespace scythe;

public struct int2(int x, int y) {
        
    public int x = x;
    public int y = y;

    public int2(float x, float y) : this((int)x, (int)y) { }
    
    public static int2 operator +(int2 a, int2 b) {
        
        return new(a.x + b.x, a.y + b.y);
    }
    
    public static int2 operator /(int2 a, int b)  {

        return new(a.x / b, a.y / b);
    }
    
    public static int2 operator /(int2 a, int2 b)  {

        return new(a.x / b.x, a.y / b.y);
    }
}

#pragma warning disable CS8981
public static partial class ext {
    
    public static int2 to_int2(this Vector2 value) {
        
        return new((int)value.X, (int)value.Y);
    }
}