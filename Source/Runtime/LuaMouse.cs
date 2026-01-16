using System.Numerics;
using Raylib_cs;

internal class LuaMouse {

    //private static KeyboardKey GetKey(string keyName) => !Enum.TryParse(keyName, out KeyboardKey key) ? throw new Exception($"Key {keyName} is not a recognized key name") : key;
    //
    //public static bool Down(string keyName) => Raylib.IsKeyDown(GetKey(keyName));
    //public static bool Pressed(string keyName) => Raylib.IsKeyPressed(GetKey(keyName));
    //public static bool Released(string keyName) => Raylib.IsKeyReleased(GetKey(keyName));
    //public static bool Up(string keyName) => Raylib.IsKeyUp(GetKey(keyName));

    public static Vector2 Delta => Raylib.GetMouseDelta();
    public static float Scroll => Raylib.GetMouseWheelMove();

    public static void SetVisible(bool visible) {

        if (visible)
             Raylib.ShowCursor();
        else Raylib.HideCursor();
    }

    public static void MoveToCenter() {

        if (!Raylib.IsWindowFocused()) return;
        
        var pos = Raylib.GetScreenCenter();
        Raylib.SetMousePosition((int)pos.X, (int)pos.Y);
    }
}