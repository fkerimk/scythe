using System.Reflection;
using System.Runtime.InteropServices;

namespace scythe;

#pragma warning disable CS8981 
public abstract class path {

    public static string dir => Environment.CurrentDirectory;
    public static string exe_dir => AppContext.BaseDirectory;
    public static string tmp_dir => Path.Combine(Path.GetTempPath(), "scythe");

    public static string relative(string path) {
        
        return process(Path.Join(dir, path));
    }
    
    public static string reverse_relative(string path) {

        return process(path.Remove(0,dir.Length - 1));
    }

    public static string process(string path) {
        
        return path.Replace('\\', '/');
    }
    
    public static void include_lib(int os, string name) {

        if (OperatingSystem.IsWindows() && os != 0) return;
        if (OperatingSystem.IsLinux() && os != 1) return;
        
        Directory.CreateDirectory(tmp_dir);

        var path = Path.Combine(tmp_dir, name);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"scythe.include.{name}"))
        using (var fs = File.Create(path))
            stream?.CopyTo(fs);

        NativeLibrary.Load(path);
    }

    public static void clear_temp() {
        
        Directory.Delete(tmp_dir, true);
    }
}