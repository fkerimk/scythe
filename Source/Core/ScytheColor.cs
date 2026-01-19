using System.ComponentModel;
using System.Numerics;

internal class ScytheColor(float r, float g, float b, float a = 1) {
    
    [DefaultValue(1f)] public readonly float R = r;
    [DefaultValue(1f)] public readonly float G = g;
    [DefaultValue(1f)] public readonly float B = b;
    [DefaultValue(1f)] public readonly float A = a;
}

internal static partial class Extensions {
    
    public static ScytheColor ToColor(this System.Drawing.Color value) => new(value.R, value.G, value.B, value.A);
  
    public static ScytheColor ToColor(this Vector4 color) => new(color.X, color.Y, color.Z, color.W);
    
    extension(ScytheColor value) {
        public Raylib_cs.Color ToRaylib() => new(value.R, value.G, value.B, value.A);

        public Vector4 to_vector4() => new(value.R, value.G, value.B, value.A);

        private byte[] ToBytes() => [(byte)(value.R * 255), (byte)(value.G * 255), (byte)(value.B * 255), (byte)(value.A * 255)];

        public uint ToUint() {

            var bytes = value.ToBytes();
            return ((uint)bytes[0] << 24) | ((uint)bytes[1]<< 16) | ((uint)bytes[2] << 8) | bytes[3];
        }
    }
}