using Raylib_cs;

internal static class Flags {

    private static ConfigFlags _activeFlags = 0;

    public static void Add(ConfigFlags flag) {
        
        _activeFlags |= flag;
        Raylib.SetConfigFlags(flag);
    }

    private static void Remove(ConfigFlags flag) {
        
        _activeFlags &= ~flag;
        Raylib.ClearWindowState(flag);
    }

    public static void Set(ConfigFlags[] flags) {
        
        Clear();
        
        foreach (var flag in flags)
            Add(flag);
    }

    private static void Clear() {

        foreach (var flag in Enum.GetValues<ConfigFlags>()) {
            
            if (_activeFlags.HasFlag(flag))
                Remove(flag);
        }
    }
}