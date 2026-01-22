internal abstract class PathUtil {
    
    public static string LaunchPath = Environment.CurrentDirectory;
    private static string CurrentPath => Environment.CurrentDirectory;
    public static string ExePath => AppContext.BaseDirectory;
    public static string TempPath = Environment.CurrentDirectory;

    public static void Init() {
        
        LaunchPath = Environment.CurrentDirectory;
        Environment.CurrentDirectory = ExePath;
    }
    
    private static string CurrentRelative(string path) => Path.GetFullPath(Path.Join(CurrentPath, path));
    private static string LaunchRelative(string path) => Path.GetFullPath(Path.Join(LaunchPath, path));
    public static string ModRelative(string path) => Path.GetFullPath(Path.Join(Config.Mod.Path, path));
    public static string ExeRelative(string path) => Path.GetFullPath(Path.Join(ExePath, path));

    public static string TempRelative(string path) {

        TempPath = Path.GetFullPath(ModRelative("Temp"));
        
        if (!Directory.Exists(TempPath))
            Directory.CreateDirectory(TempPath);
        
        return Path.Join(TempPath, path);
    }

    public static bool BestPath(string relativePath, out string path, bool isDirectory = false, bool resLock = false) {

        path = CurrentRelative(relativePath);
        if (CheckPath(path)) return true; 
        
        path = LaunchRelative(relativePath);
        if (CheckPath(path)) return true;
        
        path = ModRelative(relativePath);
        if (CheckPath(path)) return true; 
        
        path = ExeRelative(relativePath);
        if (CheckPath(path)) return true;

        if (!resLock) return BestPath(Path.Join("Resources", relativePath), out path, isDirectory,true);

        path = null!;
        return false;

        bool CheckPath(string checkPath) {
            
            switch (isDirectory) {
            
                case false when File.Exists(checkPath):
                case true when Directory.Exists(checkPath):
                    return true;
            }
            
            return false;
        }
    }
}