using System.Diagnostics;
using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Shortcuts {

    public static void Check() {
        // Ignore shortcuts if text inputs are active (like Script Editor)
        if (Editor.IsScriptEditorFocused) return;
        
        if (IsKeyDown(KeyboardKey.LeftControl)) {
            
            if (IsKeyPressed(KeyboardKey.Q)) Editor.Quit();
            if (IsKeyPressed(KeyboardKey.D)) DuplicateSelectedObject();
            if (IsKeyPressed(KeyboardKey.S)) SaveActiveLevel();
            if (IsKeyPressed(KeyboardKey.Z)) History.Undo();
            if (IsKeyPressed(KeyboardKey.Y)) History.Redo();
        }

        if (IsKeyPressed(KeyboardKey.Delete)) LevelBrowser.DeleteSelectedObject();
        if (IsKeyPressed(KeyboardKey.F5)) RunPlayMode();
    }

    private static void DuplicateSelectedObject() {
        
        if (LevelBrowser.SelectedObject == null) return;
        LevelBrowser.SelectObject(LevelBrowser.SelectedObject.CloneRecorded());
    }
    
    private static void SaveActiveLevel() {
        
        Core.ActiveLevel?.Save();
        Notifications.Show("Saved");
    }
    
    private static void RunPlayMode() {
        
        var currentPath = Process.GetCurrentProcess().MainModule?.FileName;

        if (string.IsNullOrEmpty(currentPath)) return;
        
        var psi = new ProcessStartInfo {
                
            FileName = currentPath,
            Arguments = $"nosplash cfg:Mod.Path!=\"{Config.Mod.Path}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = PathUtil.LaunchPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
            
        using var process = new Process();
            
        process.StartInfo = psi;
        process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };

        process.Start();
            
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
            
        process.WaitForExit();
    }
}