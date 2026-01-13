using System.Reflection;
using System.Runtime.InteropServices;

internal abstract class PathUtil {

    public static string Dir => Environment.CurrentDirectory;
    public static string ExeDir => AppContext.BaseDirectory;
    public static string TmpDir => Path.Combine(Path.GetTempPath(), "scythe");

    public static string Relative(string path) {
        
        return Process(Path.Join(Dir, path));
    }
    
    public static string reverse_relative(string path) {

        return Process(path.Remove(0,Dir.Length - 1));
    }

    public static string Process(string path) {
        
        return path.Replace('\\', '/');
    }
    
    public static void include_lib(int os, string name) {

        if (OperatingSystem.IsWindows() && os != 0) return;
        if (OperatingSystem.IsLinux() && os != 1) return;
        
        Directory.CreateDirectory(TmpDir);

        var path = Path.Combine(TmpDir, name);

        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"scythe.include.{name}"))
        using (var fs = File.Create(path))
            stream?.CopyTo(fs);

        NativeLibrary.Load(path);
    }

    public static void clear_temp() {

        if (Directory.Exists(TmpDir))
            Directory.Delete(TmpDir, true);
    }
}