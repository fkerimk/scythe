using System.Numerics;
using Raylib_cs;

internal static class Notifications {
    
    private const int 
        X = 25,
        Y = 25,
        Spacing = 10,
        BorderWidth = 2,
        Size = 20;
    
    private const float Fadeout = 0.125f;
    
    private static readonly List<Notification> PendingNotifications = [];

    public static void Show(string text, float duration = 0.5f) {
        
        PendingNotifications.Add(new Notification(text, duration));
    }

    public static void Draw() {
        
        if (PendingNotifications.Count == 0) return;

        var i = PendingNotifications.Count - 1;

        do {

            var notification = PendingNotifications[i];

            var text = notification.Text;

            var pos = new Vector2(X, Y + i * (Notification.Height + Spacing));

            var brColor = Colors.Back;
            var bgColor = Color.Black;
            var fgColor = Colors.Primary;

            notification.Timer += Raylib.GetFrameTime();

            brColor.A = 100;
            bgColor.A = 150;
            fgColor.A = 255;

            if (notification.Timer > notification.Duration) {

                var fadeProgress = (notification.Timer - notification.Duration) / Fadeout;
                var status = Raymath.Clamp(1 * (1f - fadeProgress), 0, 1);

                pos.X = (int)(status * X);
                
                brColor.A = (byte)(status * brColor.A);
                bgColor.A = (byte)(status * bgColor.A);
                fgColor.A = (byte)(status * fgColor.A);
                
                if (fadeProgress > 1) {

                    PendingNotifications.RemoveAt(i);
                    i -= 2;
                    continue;
                }
            }
            
            if (notification.DrawPosX == 0) notification.DrawPosX = -notification.Width - pos.X;
            if (notification.DrawPosY == 0) notification.DrawPosY = pos.Y;
            
            var finalPosX = (int)(notification.DrawPosX = Raymath.Lerp(notification.DrawPosX, pos.X, Raylib.GetFrameTime() * 15));
            var finalPosY = (int)(notification.DrawPosY = Raymath.Lerp(notification.DrawPosY, pos.Y, Raylib.GetFrameTime() * 15));

            Raylib.DrawRectangle(finalPosX, finalPosY, notification.Width, Notification.Height, brColor);
            Raylib.DrawRectangle(
                finalPosX + BorderWidth, finalPosY + BorderWidth, 
                notification.Width - BorderWidth * 2,
                Notification.Height - BorderWidth * 2,
                bgColor);
            Raylib.DrawTextEx(
                Fonts.RlMontserratRegular, 
                text,
                new Vector2(finalPosX + Notification.Height / 2f - Size / 2f, finalPosY + Notification.Height / 2f - Size / 2f),
                Size, 
                Size * 0.2f,
                fgColor);
            
            PendingNotifications[i] = notification;

            i--;

        } while (i >= 0);
    }

    private struct Notification(string text, float duration) {
    
        public readonly string Text = text;
        public readonly int Width = text.Length * (Size + (int)(Size * 0.3f)) / 2 + Size;
        public const int Height = Size * 2;
        public readonly float Duration = duration;

        public float
            Timer = 0f,
            DrawPosX,
            DrawPosY;
    }
}