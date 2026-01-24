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
        public static string RaylibLogLevel = "Warning";
    }
    
    public static class Level {

        public static string Formatting = "None";
    }

    private static readonly HashSet<string> LockedKeys = [];

    public static void Set(string keyPath, string value, bool lockKey = false) {

        var parts = keyPath.Split('.');
        if (parts.Length != 2) return;

        var fullKey = $"{parts[0]}.{parts[1]}";
        if (LockedKeys.Contains(fullKey)) return;

        var section = typeof(Config).GetNestedType(parts[0], BindingFlags.Public | BindingFlags.Static);
        if (section == null) return;

        var key = section.GetField(parts[1], BindingFlags.Public | BindingFlags.Static);
        if (key == null) return;

        if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
            value = value[1..^1];

        value = value.Replace(@"\n", "\n").Replace(@"\t", "\t").Replace(@"\\", @"\");

        var type = key.FieldType;

        if (type == typeof(int) && int.TryParse(value, out var i))
            key.SetValue(null, i);

        else if (type == typeof(bool) && bool.TryParse(value, out var b))
            key.SetValue(null, b);

        else if (type == typeof(string))
            key.SetValue(null, value);

        if (lockKey) LockedKeys.Add(fullKey);
    }

    public static void ToConfig(this Ini ini) {

        foreach (var section in typeof(Config).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            foreach (var key in section.GetFields(BindingFlags.Public | BindingFlags.Static)) {

                if (LockedKeys.Contains($"{section.Name}.{key.Name}")) continue;
                key.SetValue(null, ini.Read(section.Name, key.Name, key.GetValue(null)!));
            }
    }
}