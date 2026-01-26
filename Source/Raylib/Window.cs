using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Window {

    public static int Width  => GetScreenWidth();
    public static int Height => GetScreenHeight();

    private static int _lastFps = -2;

    public static void UpdateFps() {

        var targetFps = CommandLine.Editor ? Config.Editor.FpsLock : Config.Runtime.FpsLock;

        if (targetFps == -1) targetFps = Screen.RefreshRate;

        if (_lastFps == targetFps) return;

        _lastFps = targetFps;

        SetTargetFPS(targetFps);
    }

    public static void Clear(Color color) => ClearBackground(color);

    public static string GetFpsText() => $"{GetFPS()}";

    public static void DrawFps(System.Numerics.Vector2 pos) {

        if (CommandLine.Editor ? !Config.Editor.DrawFps : !Config.Runtime.DrawFps) return;

        var fpsText = GetFpsText();

        DrawTextEx(Fonts.RlMontserratRegular, fpsText, pos + new System.Numerics.Vector2(1, 0), 26, 1, Color.Black);
        DrawTextEx(Fonts.RlMontserratRegular, fpsText, pos,                                     26, 1, Colors.Primary);
    }

    private static void CenterWindow() => SetWindowPosition((Screen.Width - Width) / 2, (Screen.Height - Height) / 2);

    public static void Show(int width = 0, int height = 0, float scale = 0.75f, bool maximize = false, bool fullscreen = false, bool borderless = true, string title = "SCYTHE", bool isSplash = false, params ConfigFlags[] flags) {

        // Close old window
        if (IsWindowReady()) CloseWindow();

        // Flags
        Flags.Set(flags);

        // New window     
        InitWindow(0, 0, title);

        if (width  == 0) width  = Screen.Width;
        if (height == 0) height = Screen.Height;
        width  = (int)Math.Floor(width  * scale);
        height = (int)Math.Floor(height * scale);

        SetWindowSize(width, height);

        SetExitKey(KeyboardKey.Null);

        CenterWindow();

        // Fullscreen & maximizing 
        if (fullscreen) {

            if (borderless) {

                Flags.Add(ConfigFlags.UndecoratedWindow);
                SetWindowSize(GetMonitorWidth(GetCurrentMonitor()), GetMonitorHeight(GetCurrentMonitor()));
                CenterWindow();
            } else if (!IsWindowFullscreen()) ToggleFullscreen();

        } else if (maximize) MaximizeWindow();

        // Window icon
        if (PathUtil.BestPath("Images/Icon.png", out var iconPath)) {

            var img = LoadImage(iconPath);
            SetWindowIcon(img);
            UnloadImage(img);
        }

        // Initialize Audio
        if (!IsAudioDeviceReady()) InitAudioDevice();
    }
}