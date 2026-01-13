using System.ComponentModel;
using System.Numerics;

internal class Color(float r, float g, float b, float a = 1) {
    
    [DefaultValue(1f)] public float R = r;
    [DefaultValue(1f)] public float G = g;
    [DefaultValue(1f)] public float B = b;
    [DefaultValue(1f)] public float A = a;
}

internal static partial class Extensions {
    
    public static Color ToColor(this System.Drawing.Color value) => new(value.R, value.G, value.B, value.A);
  
    public static Color ToColor(this Vector4 color) => new(color.X, color.Y, color.Z, color.W);
    
    extension(Color value) {
        public Raylib_cs.Color ToRaylib() => new(value.R, value.G, value.B, value.A);

        public Vector4 to_vector4() => new(value.R, value.G, value.B, value.A);

        public byte4 ToBytes() => new((byte)(value.R * 255), (byte)(value.G * 255), (byte)(value.B * 255), (byte)(value.A * 255));

        public uint ToUint() {

            var bytes = value.ToBytes();
            return ((uint)bytes.x << 24) | ((uint)bytes.y << 16) | ((uint)bytes.z << 8) | bytes.w;
        }
    }
}