namespace scythe;

#pragma warning disable CS8981 
public abstract class path {

    public static string dir => Environment.CurrentDirectory;
    public static string exe_dir => AppContext.BaseDirectory;

    public static string relative(string path) {
        
        return process(Path.Join(dir, path));
    }
    
    public static string reverse_relative(string path) {

        return process(path.Remove(0,dir.Length - 1));
    }

    public static string process(string path) {
        
        return path.Replace('\\', '/').TrimStart('/');
    }
}