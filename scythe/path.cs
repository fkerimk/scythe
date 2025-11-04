namespace scythe;

#pragma warning disable CS8981 
public abstract class path {

    public static string exe_dir => AppContext.BaseDirectory;

    public static string relative(string path) {
        
        return process(Path.Join(exe_dir, path));
    }
    
    public static string reverse_relative(string path) {

        return process(path.Remove(0,exe_dir.Length - 1));
    }

    public static string process(string path) {
        
        return path.Replace('\\', '/').TrimStart('/');
    }
}