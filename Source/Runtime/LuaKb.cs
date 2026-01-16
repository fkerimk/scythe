using Raylib_cs;

internal class LuaKb {

    private static KeyboardKey GetKey(string keyName) => !Enum.TryParse(keyName, out KeyboardKey key) ? throw new Exception($"Key {keyName} is not a recognized key name") : key;
    
    public static bool Down(string keyName) => Raylib.IsKeyDown(GetKey(keyName));
    public static bool Pressed(string keyName) => Raylib.IsKeyPressed(GetKey(keyName));
    public static bool Released(string keyName) => Raylib.IsKeyReleased(GetKey(keyName));
    public static bool Up(string keyName) => Raylib.IsKeyUp(GetKey(keyName));
}