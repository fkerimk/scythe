using System.Reflection;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

internal static class Config {
    
    public static class Mod {
        
        public static string Name = "SCYTHE";
        public static string Path = "";
    }
    
    public static class Runtime {
        
        public static int FpsLock = -1;
        public static bool DrawFps = false;
        public static bool DrawLights = false;
        public static bool NoShade = false;
        public static int PbrAlbedo = 1;
        public static int PbrNormal = 1;
        public static int PbrMra = 1;
        public static int PbrEmissive = 1;
        public static bool GenTangents = true;
        public static string RaylibLogLevel = "Error";
    }
    
    public static class Editor {
        
        public static int FpsLock = -1;
        public static bool DrawFps = true;
        public static bool DrawLights = true;
        public static bool NoShade = false;
        public static int PbrAlbedo = 1;
        public static int PbrNormal = 1;
        public static int PbrMra = 1;
        public static int PbrEmissive = 1;
        public static bool GenTangents = true;
        public static string RaylibLogLevel = "Error";
    }
    
    public static class Level {

        public static string Formatting = "None";
    }

    public static void ToConfig(this Ini ini) {

        foreach (var section in typeof(Config).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            foreach (var key in section.GetFields(BindingFlags.Public | BindingFlags.Static))
                key.SetValue(null, ini.Read(section.Name, key.Name, key.GetValue(null)!));
    }
}