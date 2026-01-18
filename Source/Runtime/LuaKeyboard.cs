using Raylib_cs;
using static Raylib_cs.Raylib;

internal class LuaKb {

    private static KeyboardKey GetKey(string keyName) => !Enum.TryParse(keyName, out KeyboardKey key) ? throw new Exception($"Key {keyName} is not a recognized key name") : key;
    
    public static bool Down(string keyName) => IsKeyDown(GetKey(keyName));
    public static bool Pressed(string keyName) => IsKeyPressed(GetKey(keyName));
    public static bool Released(string keyName) => IsKeyReleased(GetKey(keyName));
    public static bool Up(string keyName) => IsKeyUp(GetKey(keyName));
}