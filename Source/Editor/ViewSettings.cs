using Newtonsoft.Json;

internal static class ViewSettings {
    
    private static string GetPath() => PathUtil.BestPath("Layouts/Viewports.json", out var path) ? path : PathUtil.ExeRelative("Layouts/Viewports.json");

    public static void Save() {
        
        var path = GetPath();
        
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var settings = new Dictionary<string, bool> {
            
            { "Level 3D", Editor.Level3D.IsOpen },
            { "Level Browser", Editor.LevelBrowser.IsOpen },
            { "Object Browser", Editor.ObjectBrowser.IsOpen },
            { "Project Browser", Editor.ProjectBrowser.IsOpen }
        };

        var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static void Load() {
        
        var path = GetPath();
        if (!File.Exists(path)) return;

        try {
            
            var json = File.ReadAllText(path);
            var settings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
            
            if (settings == null) return;

            if (settings.TryGetValue("Level 3D", out var l3d)) Editor.Level3D.IsOpen = l3d;
            if (settings.TryGetValue("Level Browser", out var lb)) Editor.LevelBrowser.IsOpen = lb;
            if (settings.TryGetValue("Object Browser", out var ob)) Editor.ObjectBrowser.IsOpen = ob;
            if (settings.TryGetValue("Project Browser", out var pb)) Editor.ProjectBrowser.IsOpen = pb;
            
        } catch {
            
            // ignored
        }
    }
}
