using System.Runtime.InteropServices;

internal static class NativeResolver {
    
    public static void Init() {
        
        NativeLibrary.SetDllImportResolver(typeof(Assimp.AssimpContext).Assembly, (libraryName, _, _) => {
            
            if (libraryName != "assimp") return IntPtr.Zero;

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                
                path = Path.Combine(baseDir, "runtimes", "win-x64", "native", "assimp.dll");
                if (!File.Exists(path)) path = Path.Combine(baseDir, "assimp.dll");
            }
            
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                
                path = Path.Combine(baseDir, "runtimes", "linux-x64", "native", "libassimp.so");
                if (!File.Exists(path)) path = Path.Combine(baseDir, "libassimp.so");
            }

            return File.Exists(path) ? NativeLibrary.Load(path) : IntPtr.Zero;
        });
    }
}
