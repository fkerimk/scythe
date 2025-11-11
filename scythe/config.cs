using System.Reflection;

namespace scythe;

public static class config {
    
    public static class mod {
        
        public static string name = "SCYTHE";
        public static string path = "";
    }

    public static class runtime {
        
        public static int fps_lock = -1;
        public static bool draw_fps = false;
        public static bool draw_lights = false;
        public static bool no_shade = false;
        public static int pbr_albedo = 1;
        public static int pbr_normal = 1;
        public static int pbr_mra = 1;
        public static int pbr_emissive = 1;
        public static bool gen_tangents = true;
    }
    
    public static class editor {
        
        public static int fps_lock = -1;
        public static bool draw_fps = true;
        public static bool draw_lights = true;
        public static bool no_shade = false;
        public static int pbr_albedo = 1;
        public static int pbr_normal = 1;
        public static int pbr_mra = 1;
        public static int pbr_emissive = 1;
        public static bool gen_tangents = true;
    }

    public static void to_config( this ini ini) {

        foreach (var section in typeof(config).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            foreach (var key in section.GetFields(BindingFlags.Public | BindingFlags.Static))
                key.SetValue(null, ini.read(section.Name, key.Name, key.GetValue(null)!));
    }
}