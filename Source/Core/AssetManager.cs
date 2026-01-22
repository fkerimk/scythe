internal static class AssetManager {
    
    private static readonly Dictionary<string, Asset> Assets = new();

    public static bool Load<T>(string path, out T? asset) where T : Asset, new() {

        var key = path + typeof(T).Name;

        if (Assets.TryGetValue(key, out var existing) && existing is T typedAsset && existing.IsLoaded) {
            
            asset = typedAsset;
            return true;
        }
        
        var newAsset = new T { File = path };
        Assets[key] = newAsset;

        if (newAsset.Load()) {
            
            asset = newAsset;
            return true;
        }
        
        asset = null;
        return false;
    }

    private static void Unload(string path) {
        
        if (Assets.TryGetValue(path, out var asset) && asset.IsLoaded)
            asset.Unload();
    }

    public static void UnloadAll() {

        foreach (var asset in Assets.Keys)
            Unload(asset);
    }
}