using static Raylib_cs.Raylib;

internal class LuaTime {

    public static float Delta => GetFrameTime();
    public static float Passed => (float)GetTime();
}