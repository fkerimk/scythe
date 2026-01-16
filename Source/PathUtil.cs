internal abstract class PathUtil {
    
    public static string LaunchPath = Environment.CurrentDirectory;
    private static string CurrentPath => Environment.CurrentDirectory;
    private static string ExePath => AppContext.BaseDirectory;

    public static string CurrentRelative(string path) => Process(Path.Join(CurrentPath, path));
    public static string LaunchRelative(string path) => Process(Path.Join(LaunchPath, path));
    public static string ModRelative(string path) => Process(Path.Join(Config.Mod.Path, path));
    public static string ExeRelative(string path) => Process(Path.Join(ExePath, path));

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
                    //Console.WriteLine("PASS: " + checkPath);
                    return true;
            }
            
            //Console.WriteLine("FAIL: " + checkPath);
            
            return false;
        }
    }

    private static string Process(string path) => path.Replace('\\', '/');
}