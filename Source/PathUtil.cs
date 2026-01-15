internal abstract class PathUtil {

    public static string FirstDir = Environment.CurrentDirectory;
    public static string ExeDir => AppContext.BaseDirectory;
    
    private static string Dir => Environment.CurrentDirectory;

    public static string Relative(string path) => Process(Path.Join(Dir, path));

    private static string Process(string path) => path.Replace('\\', '/');
}