using System.Numerics;
using System.Transactions;
using static Raylib_cs.Raylib;

internal class LuaMouse {

    //private static KeyboardKey GetKey(string keyName) => !Enum.TryParse(keyName, out KeyboardKey key) ? throw new Exception($"Key {keyName} is not a recognized key name") : key;
    //
    //public static bool Down(string keyName) => Raylib.IsKeyDown(GetKey(keyName));
    //public static bool Pressed(string keyName) => Raylib.IsKeyPressed(GetKey(keyName));
    //public static bool Released(string keyName) => Raylib.IsKeyReleased(GetKey(keyName));
    //public static bool Up(string keyName) => Raylib.IsKeyUp(GetKey(keyName));

    public static bool IsLocked;
    
    public static Vector2 Delta;
    public static float Scroll => GetMouseWheelMove();

    public static void Loop() {

        if (IsCursorOnScreen() && IsWindowFocused()) {
            
            Delta = GetMouseDelta();
            
            if (IsCursorHidden() && !IsLocked) ShowCursor();
            if (!IsCursorHidden() && IsLocked) HideCursor();

            if (IsLocked) MoveToCenter();

        } else {
            
            Delta = Vector2.Zero;
            
            ShowCursor();
            EnableCursor();
        }
    }
    
    public static void SetVisible(bool visible) {

        if (visible)
             ShowCursor();
        else HideCursor();
    }

    public static void MoveToCenter() {

        if (!IsWindowFocused()) return;
        
        var pos = GetScreenCenter();
        SetMousePosition((int)pos.X, (int)pos.Y);
    }
}