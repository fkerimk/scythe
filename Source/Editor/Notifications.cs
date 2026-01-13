using System.Numerics;
using Raylib_cs;

internal static class Notifications {
    
    private const int 
        X = 25,
        Y = 25,
        Spacing = 10,
        BorderWidth = 2,
        Size = 20;
    
    private const float 
        Duration = 0.5f,
        Fadeout = 0.125f;
    
    private static readonly List<Notification> PendingNotifications = [];

    public static void Show(string text) {
        
        PendingNotifications.Add(new Notification(text));
    }

    public static void Draw() {
        
        if (PendingNotifications.Count == 0) return;

        var i = PendingNotifications.Count - 1;

        do {

            var notification = PendingNotifications[i];

            var text = notification.Text;

            var pos = new int2(X, Y + i * (notification.Height + Spacing));

            var brColor = Colors.Back.ToRaylib();
            var bgColor = Colors.Black.ToRaylib();
            var fgColor = Colors.Primary.ToRaylib();

            notification.Timer += Raylib.GetFrameTime();

            brColor.A = 100;
            bgColor.A = 150;
            fgColor.A = 255;

            if (notification.Timer > Duration) {

                var fadeProgress = (notification.Timer - Duration) / Fadeout;
                var status = Raymath.Clamp(1 * (1f - fadeProgress), 0, 1);

                pos.x = (int)(status * X);
                
                brColor.A = (byte)(status * brColor.A);
                bgColor.A = (byte)(status * bgColor.A);
                fgColor.A = (byte)(status * fgColor.A);
                
                if (fadeProgress > 1) {

                    PendingNotifications.RemoveAt(i);
                    i -= 2;
                    continue;
                }
            }
            
            if (notification.DrawPosX == 0) notification.DrawPosX = -notification.Width - pos.x;
            if (notification.DrawPosY == 0) notification.DrawPosY = pos.y;
            
            var finalPosX = (int)(notification.DrawPosX = Raymath.Lerp(notification.DrawPosX, pos.x, Raylib.GetFrameTime() * 15));
            var finalPosY = (int)(notification.DrawPosY = Raymath.Lerp(notification.DrawPosY, pos.y, Raylib.GetFrameTime() * 15));

            Raylib.DrawRectangle(finalPosX, finalPosY, notification.Width, notification.Height, brColor);
            Raylib.DrawRectangle(
                finalPosX + BorderWidth, finalPosY + BorderWidth, 
                notification.Width - BorderWidth * 2,
                notification.Height - BorderWidth * 2,
                bgColor);
            Raylib.DrawTextEx(
                Fonts.RlMontserratRegular, 
                text,
                new Vector2(finalPosX + notification.Height / 2f - Size / 2f, finalPosY + notification.Height / 2f - Size / 2f),
                Size, 
                Size * 0.2f,
                fgColor);
            
            PendingNotifications[i] = notification;

            i--;

        } while (i >= 0);
    }

    private struct Notification(string text) {
    
        public readonly string Text = text;
        public readonly int Width = text.Length * (Size + (int)(Size * 0.3f)) / 2 + Size;
        public readonly int Height = Size * 2;
        
        public float
            Timer = 0f,
            DrawPosX,
            DrawPosY;
    }
}