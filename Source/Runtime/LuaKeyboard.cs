using Raylib_cs;
using static Raylib_cs.Raylib;

internal class LuaKb {
    
    public bool Down(LuaKey key) => IsKeyDown((KeyboardKey)key);
    public bool Pressed(LuaKey key) => IsKeyPressed((KeyboardKey)key);
    public bool Released(LuaKey key) => IsKeyReleased((KeyboardKey)key);
    public bool Up(LuaKey key) => IsKeyUp((KeyboardKey)key);
}