using System.Numerics;
using static Raylib_cs.Raylib;

internal class LuaMouse {

    public static readonly bool IsLocked = false;
    
    public static Vector2 Delta;
    public static float Scroll => GetMouseWheelMove();

    public static void Loop() {

        if (!IsWindowFocused()) {
            if (IsCursorHidden()) {
                EnableCursor();
                ShowCursor();
            }
            Delta = Vector2.Zero;
            return;
        }

        Delta = GetMouseDelta();

        if (IsLocked) {
            
            if (!IsCursorHidden()) 
                DisableCursor();
        } 
        
        else {
            
            if (IsCursorHidden()) {
                EnableCursor();
                ShowCursor();
            }
        }
    }
    
    public static void SetVisible(bool visible) {

        if (visible)
             ShowCursor();
        else HideCursor();
    }

    public static void MoveToCenter() {

        if (!IsWindowFocused()) return;
        
        var pos = new Vector2(GetScreenWidth() / 2f, GetScreenHeight() / 2f);
        SetMousePosition((int)pos.X, (int)pos.Y);
    }
}