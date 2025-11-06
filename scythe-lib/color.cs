namespace scythe;

#pragma warning disable CS8981
public class color(float r, float g, float b, float a = 1) {
    
    public float r = r;
    public float g = g;
    public float b = b;
    public float a = a;
}

#pragma warning disable CS8981
public static partial class ext {
    
    public static Raylib_cs.Color to_raylib(this color value) {
        
        return new(value.r, value.g, value.b, value.a);
    }
    
    public static color to_color(this System.Drawing.Color value) {
        
        return new(value.R, value.G, value.B, value.A);
    }
    
    public static System.Numerics.Vector4 to_vector4(this color color) {
        
        return new(color.r, color.g, color.b, color.a);
    }
    
    public static byte4 to_bytes(this color color) {
        
        return new((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
    }
    
    public static uint to_uint(this color color) {

        var bytes = color.to_bytes();
        
        return ((uint)bytes.x << 24) | ((uint)bytes.y << 16) | ((uint)bytes.z << 8) | (uint)bytes.w;
    }
}