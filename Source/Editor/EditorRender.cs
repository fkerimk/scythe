using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using Newtonsoft.Json;

internal class EditorRender() : Viewport("Render (Editor)") {

    public RenderTexture2D Rt = new(), OutlineRt = new();
    public Vector2 TexSize = Vector2.One, TexTemp = Vector2.Zero;
    public static Vector2 RelativeMouse3D;

    private static string GetPath() => PathUtil.ProjectRelative("EditorRender.json");

    public void Load() {
        
        var path = GetPath();
        if (File.Exists(path)) {

            SafeExec.Try(() => {
                
                var settings = JsonConvert.DeserializeObject<EditorRenderSettings>(File.ReadAllText(path));
                if (settings == null) return;

                foreach (var relPath in settings.OpenLevels) {
                    
                    var absPath = PathUtil.ModRelative(relPath);
                    if (File.Exists(absPath)) Editor.OpenLevel(absPath);
                }

                if (settings.ActiveLevelIndex >= 0 && settings.ActiveLevelIndex < Core.OpenLevels.Count)
                    Core.SetActiveLevel(settings.ActiveLevelIndex);
            });
        }

        EnsureAtLeastOneLevelOpen();
    }

    private void EnsureAtLeastOneLevelOpen() {
        
        if (Core.OpenLevels.Count > 0) return;
        
        if (PathUtil.BestPath("Levels/Main.json", out var mainPath)) {
             Editor.OpenLevel(mainPath);
        } else {
            
            var levelsDir = PathUtil.ModRelative("Levels");
            if (Directory.Exists(levelsDir)) {
                
                var firstJson = Directory.GetFiles(levelsDir, "*.json", SearchOption.AllDirectories).FirstOrDefault();
                
                if (firstJson != null) Editor.OpenLevel(firstJson);
                else Editor.CreateLevel(Path.Combine(levelsDir, "Main.json"));
                
            } else {
                
                Directory.CreateDirectory(levelsDir);
                Editor.CreateLevel(Path.Combine(levelsDir, "Main.json"));
            }
        }
    }

    public void Save() {
        
        var path = GetPath();
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var settings = new EditorRenderSettings {
            
            OpenLevels = Core.OpenLevels.Select(l => Path.GetRelativePath(Config.Mod.Path, l.JsonPath).Replace('\\', '/')).ToList(),
            ActiveLevelIndex = Core.ActiveLevelIndex
        };

        File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
    }

    private class EditorRenderSettings {
        
        public List<string> OpenLevels { get; init; } = [];
        public int ActiveLevelIndex { get; init; } = -1;
    }

    protected override void OnDraw() {

        EnsureAtLeastOneLevelOpen();

        if (Core.OpenLevels.Count == 0) return;

        if (ImGui.BeginTabBar("##LevelTabs", ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.Reorderable)) {

            for (int i = 0; i < Core.OpenLevels.Count; i++) {

                var level = Core.OpenLevels[i];
                var label = $"{level.Name}{(level.IsDirty ? " *" : "")}###level_{level.GetHashCode()}";
                
                bool open = true;
                bool isSelected = (Core.ActiveLevelIndex == i);
                var flags = ImGuiTabItemFlags.None;
                
                if (Core.ShouldFocusActiveLevel && isSelected) {
                    flags |= ImGuiTabItemFlags.SetSelected;
                    // Only clear the flag after we've processed all tabs or at least the one we want to focus
                }

                if (ImGui.BeginTabItem(label, ref open, flags)) {

                    if (!isSelected) Core.SetActiveLevel(i);

                    if (Rt.Texture is { Width: > 0, Height: > 0 }) {
                        
                        var tex = (IntPtr)Rt.Texture.Id;
                        var contentPos = ImGui.GetCursorScreenPos();
                        var avail = ImGui.GetContentRegionAvail();
                        
                        ImGui.Image(tex, avail, new Vector2(0, 1), new Vector2(1, 0));

                        var mouse = Raylib.GetMousePosition();
                        var relX = Raymath.Clamp((mouse.X - contentPos.X) / avail.X, 0, 1);
                        var relY = Raymath.Clamp((mouse.Y - contentPos.Y) / avail.Y, 0, 1);

                        relX = (relX - 0.5f) * (avail.X / Raylib.GetScreenWidth()) * (Raylib.GetScreenHeight() / avail.Y) + 0.5f;
                        RelativeMouse3D = new Vector2(relX * Raylib.GetScreenWidth(), relY * Raylib.GetScreenHeight());
                        
                        TexSize = avail;
                    }

                    ImGui.EndTabItem();
                }

                if (open) continue;
                
                Core.CloseLevel(i);
                break;
            }

            ImGui.EndTabBar();
        }
        
        Core.ShouldFocusActiveLevel = false;
    }
}