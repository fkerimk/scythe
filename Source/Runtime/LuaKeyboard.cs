using Raylib_cs;
using static Raylib_cs.Raylib;

internal class LuaKb {

    private KeyboardKey GetKey(string keyName) {

        if (Enum.TryParse(keyName, true, out KeyboardKey key)) return key;

        // Handle single letters strictly
        if (keyName.Length == 1 && Enum.TryParse(keyName.ToUpper(), true, out key)) return key;

        throw new Exception($"Key {keyName} is not a recognized key name");
    }

    public bool Down(string keyName) => IsKeyDown(GetKey(keyName));
    public bool Pressed(string keyName) => IsKeyPressed(GetKey(keyName));
    public bool Released(string keyName) => IsKeyReleased(GetKey(keyName));
    public bool Up(string keyName) => IsKeyUp(GetKey(keyName));
}