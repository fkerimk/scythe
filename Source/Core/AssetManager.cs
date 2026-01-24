using System.Collections.Concurrent;
using Newtonsoft.Json;

internal static class AssetManager {
    
    private static readonly Dictionary<string, Asset> Assets = new();
    private static readonly List<FileSystemWatcher> Watchers = [];
    private static readonly Dictionary<string, Asset> PathLookup = new();
    private static readonly Dictionary<Type, List<Asset>> TypeCache = new();
    private static readonly ConcurrentQueue<Action> PendingActions = new();

    public static void Update() {
        
        while (PendingActions.TryDequeue(out var action))
            action();
    }

    public static void Init() {
        
        PathLookup.Clear();
        Assets.Clear();
        TypeCache.Clear();

        // 1. Load Resources
        if (PathUtil.BestPath("Resources", out var resourcesPath, isDirectory: true)) {
            
             ScanDirectory(resourcesPath);
             CreateWatcher(resourcesPath, "*.*", HandleFileChange, HandleFileDelete);
        }

        // 2. Mod Directories
        var dirs = new[] { "Models", "Scripts", "Shaders", "Images" };
        
        foreach (var d in dirs) {
            
            var path = PathUtil.ModRelative(d);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            ScanDirectory(path);
            CreateWatcher(path, "*.*", HandleFileChange, HandleFileDelete);
        }
    }
    
    private static void ScanDirectory(string path) {
        
        if (!Directory.Exists(path)) return;
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files) ImportFile(Path.GetFullPath(file));
    }

    private static void HandleFileChange(string file) => ImportFile(file);
    private static void HandleFileDelete(string file) => UnloadAsset(file);

    private static void ImportFile(string file) {
        
        var ext = Path.GetExtension(file).ToLowerInvariant();
        
        switch (ext) {
            case ".fbx":
            case ".obj":
            case ".gltf":
                ImportModel(file);
                break;
            case ".lua":
                ImportScript(file);
                break;
            case ".vs":
            case ".fs":
                ImportShader(file);
                break;
            default: {
                if (file.EndsWith(".material.json", StringComparison.OrdinalIgnoreCase)) ImportMaterial(file);
                else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga" || ext == ".bmp") ImportTexture(file);
                else if (ext == ".json") {
                    var assetFile = file[..^5];
                    if (File.Exists(assetFile)) ImportFile(assetFile);
                }

                break;
            }
        }
    }

    private static void UnloadAsset(string file) {
        
        var path = file.Replace('\\', '/').ToLowerInvariant();
        var toRemove = Assets.Where(kvp => kvp.Value.File.Replace('\\', '/').ToLowerInvariant() == path).ToList();
        
        foreach (var kvp in toRemove) {
            
            kvp.Value.Unload();
            Assets.Remove(kvp.Key);
            RemoveFromMaps(kvp.Value);
        }
    }
    
    private static void RemoveFromMaps(Asset asset) {
        
        var keysToRemove = PathLookup.Where(kvp => kvp.Value == asset).Select(kvp => kvp.Key).ToList();
        foreach (var k in keysToRemove) PathLookup.Remove(k);
        if (TypeCache.TryGetValue(asset.GetType(), out var list)) list.Remove(asset);
    }

    private static void CreateWatcher(string path, string filter, Action<string> onImport, Action<string> onUnload) {
        
        var watcher = new FileSystemWatcher(path, filter) { IncludeSubdirectories = true };
        
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
        watcher.Changed += (_, e) => PendingActions.Enqueue(() => { SafeExec.Try(() => onImport(e.FullPath)); });
        watcher.Created += (_, e) => PendingActions.Enqueue(() => { SafeExec.Try(() => onImport(e.FullPath)); });
        watcher.Deleted += (_, e) => PendingActions.Enqueue(() => { SafeExec.Try(() => onUnload(e.FullPath)); });
        watcher.Renamed += (_, e) => PendingActions.Enqueue(() => { SafeExec.Try(() => { onUnload(e.OldFullPath); onImport(e.FullPath); }); });
        watcher.EnableRaisingEvents = true;
        
        Watchers.Add(watcher);
    }
    
    private static void ImportModel(string file) {
        
        var oldJson = Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file) + ".json");
        var newJson = file + ".json";
        
        if (File.Exists(oldJson) && !File.Exists(newJson) && oldJson != newJson)
            SafeExec.Try(() => File.Move(oldJson, newJson));
        
        if (!File.Exists(newJson))
            SafeExec.Try(() => File.WriteAllText(newJson, JsonConvert.SerializeObject(new ModelAsset.ModelSettings(), Formatting.Indented)));
        
        GetOrLoad<ModelAsset>(file);
        GetOrLoad<AnimationAsset>(file);
    }

    private static void ImportScript(string file) => GetOrLoad<ScriptAsset>(file);
    
    private static void ImportMaterial(string file) => GetOrLoad<MaterialAsset>(file);
    
    private static void ImportTexture(string file) => GetOrLoad<TextureAsset>(file);
    
    private static void ImportShader(string file) {
        
        if (file.EndsWith(".fs", StringComparison.OrdinalIgnoreCase)) {
            
            var vs = Path.ChangeExtension(file, ".vs");
            if (File.Exists(vs)) return;
        }
        
        GetOrLoad<ShaderAsset>(file);
    }

    private static void GetOrLoad<T>(string file) where T : Asset, new() {
        
        var key = $"{file.ToLowerInvariant()}::{typeof(T).Name}";
        
        if (!Assets.TryGetValue(key, out var asset)) {
            
            asset = new T { File = file };
            Assets[key] = asset;
            AddToMaps<T>(file, asset);
        }
        
        if (!asset.IsLoaded) asset.Load();
    }

    private static void AddToMaps<T>(string file, Asset asset) {
        
        var typePrefix = typeof(T).Name + "::";
        var full = Path.GetFullPath(file).Replace('\\', '/').ToLowerInvariant();
        var name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
        
        PathLookup[typePrefix + full] = asset;
        PathLookup[typePrefix + name] = asset;
        
        if (full.Contains(Config.Mod.Path.Replace('\\', '/'), StringComparison.InvariantCultureIgnoreCase)) {
            
            var rel = Path.GetRelativePath(Config.Mod.Path, file).Replace('\\', '/').ToLowerInvariant();
            PathLookup[typePrefix + rel] = asset;
        }

        if (!TypeCache.TryGetValue(typeof(T), out var list)) {
            
            list = (List<Asset>)[];
            TypeCache[typeof(T)] = list;
        }
        
        if (!list.Contains(asset)) list.Add(asset);
    }

    public static T? Get<T>(string? name) where T : Asset {
        
        if (string.IsNullOrEmpty(name)) return null;
        
        var req = name.Replace('\\', '/').ToLowerInvariant();
        var prefix = typeof(T).Name + "::";
        
        if (PathLookup.TryGetValue(prefix + req, out var asset) && asset is T { IsLoaded: true } tAsset) return tAsset;
        
        // Fallback for resources
        if (req.Contains(':') || req.StartsWith('/')) return null;
        
        var res = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", name)).Replace('\\', '/').ToLowerInvariant();
        if (PathLookup.TryGetValue(prefix + res, out var rAsset) && rAsset is T { IsLoaded: true } rtAsset) return rtAsset;

        return null;
    }

    public static List<T> GetAssets<T>() where T : Asset {
        
        return TypeCache.TryGetValue(typeof(T), out var list)
            ? list.Cast<T>().ToList()
            : [];
    }

    public static List<(string Name, string Path)> GetNames<T>() where T : Asset {
        
        return TypeCache.TryGetValue(typeof(T), out var list)
            ? list.Cast<T>().Select(a => (Path.GetFileNameWithoutExtension(a.File), a.File)).OrderBy(n => n.Item1).ToList()
            : [];
    }

    public static void UnloadAll() {
        
        foreach (var watcher in Watchers)
            watcher.Dispose();
        
        Watchers.Clear();
        
        foreach (var asset in Assets.Values)
            asset.Unload();
        
        Assets.Clear();
        PathLookup.Clear();
        TypeCache.Clear();
    }
}