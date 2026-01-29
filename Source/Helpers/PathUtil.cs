internal static class PathUtil {

    public static void ValidateFile(string path, out string validPath, string content = "", bool project = false) {

        validPath = path;

        if (File.Exists(validPath)) return;

        validPath = Path.Join(ScytheConfig.Current.Project, path);

        if (File.Exists(validPath)) return;

        if (!project) {

            validPath = Path.Join(AppContext.BaseDirectory, path);

            if (File.Exists(validPath)) return;
        }

        ValidateDir(Path.GetDirectoryName(validPath)!, out _, project);

        File.WriteAllText(validPath, content);
    }

    public static void ValidateDir(string path, out string validPath, bool project = false) {

        validPath = path;

        if (Directory.Exists(validPath)) return;

        validPath = Path.Join(ScytheConfig.Current.Project, path);

        if (Directory.Exists(validPath)) return;

        if (!project) {

            validPath = Path.Join(AppContext.BaseDirectory, path);

            if (Directory.Exists(validPath)) return;
        }

        Directory.CreateDirectory(validPath);
    }

    public static bool GetPath(string relativePath, out string fullPath) {

        fullPath = Path.GetFullPath(relativePath);

        if (File.Exists(fullPath) || Directory.Exists(fullPath)) return true;

        fullPath = Path.Join(ScytheConfig.Current.Project, relativePath);

        if (File.Exists(fullPath) || Directory.Exists(fullPath)) return true;

        fullPath = Path.Join(Path.GetFullPath("Resources"), relativePath);

        if (File.Exists(fullPath) || Directory.Exists(fullPath)) return true;

        return false;
    }
}