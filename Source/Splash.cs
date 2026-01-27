using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

internal static class Splash {

    private const float Duration = 1;

    private static float     _time;
    private static Texture2D _art;

    public static void Show() {

        if (!PathUtil.BestPath("Images/Splash.png", out var splashPath)) return;

        Window.Show(scale: 1, width: 320, height: 190, flags: ConfigFlags.UndecoratedWindow, isSplash: true);

        _art = LoadTexture(splashPath);

        if (CommandLine.Editor) LspInstaller.Start();

        while (!WindowShouldClose()) {

            Window.UpdateFps();

            BeginDrawing();

            DrawTexturePro(_art, new Rectangle(0, 0, _art.Width, _art.Height), new Rectangle(0, 0, Window.Width, Window.Height), Vector2.Zero, 0, Color.White);

            _time += GetFrameTime();

            if (CommandLine.Editor && !LspInstaller.IsDone) {

                DrawRectangle(0, Window.Height - 40, Window.Width, 40, new Color(0, 0, 0, 200));

                DrawText(LspInstaller.Status, 10, Window.Height - 32, 10, Color.White);

                if (LspInstaller.Progress > 0) DrawRectangle(10, Window.Height - 14, (int)((Window.Width - 20) * LspInstaller.Progress), 4, Color.SkyBlue);

                var spinnerTime = (float)GetTime() * 10;
                var cx          = Window.Width  - 20;
                var cy          = Window.Height - 20;

                for (var i = 0; i < 8; i++) {
                    var angle = spinnerTime + i * (360f / 8f);
                    var rad   = angle * (Math.PI / 180f);
                    var x     = cx + Math.Cos(rad) * 8;
                    var y     = cy + Math.Sin(rad) * 8;
                    DrawCircle((int)x, (int)y, 2, Fade(Color.White, i / 8f));
                }

            } else if (_time >= Duration) break;

            EndDrawing();
        }

        UnloadTexture(_art);
        CloseWindow();
    }
}