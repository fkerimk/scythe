using Raylib_cs;

internal class LuaTime {

    public static float Delta => Raylib.GetFrameTime();
    public static float Passed => (float)Raylib.GetTime();
}