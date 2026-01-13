using System.ComponentModel;

internal class Color(float r, float g, float b, float a = 1) {
    
    [DefaultValue(1f)] public float R = r;
    [DefaultValue(1f)] public float G = g;
    [DefaultValue(1f)] public float B = b;
    [DefaultValue(1f)] public float A = a;
}

#pragma warning disable CS8981
internal static partial class Extensions {
    
    public static Raylib_cs.Color to_raylib(this Color value) {
        
        return new(value.R, value.G, value.B, value.A);
    }
    
    public static Color to_color(this System.Drawing.Color value) {
        
        return new(value.R, value.G, value.B, value.A);
    }
  
    public static System.Numerics.Vector4 to_vector4(this Color color) {
        
        return new(color.R, color.G, color.B, color.A);
    }
    
    public static Color to_color(this System.Numerics.Vector4 color) {
        
        return new(color.X, color.Y, color.Z, color.W);
    }
    
    public static byte4 to_bytes(this Color color) {
        
        return new((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255), (byte)(color.A * 255));
    }
    
    public static uint to_uint(this Color color) {

        var bytes = color.to_bytes();
        
        return ((uint)bytes.x << 24) | ((uint)bytes.y << 16) | ((uint)bytes.z << 8) | bytes.w;
    }
}