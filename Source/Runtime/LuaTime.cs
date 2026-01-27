using static Raylib_cs.Raylib;

internal class LuaTime {

    private static double _offset;

    public static float Delta  => GetFrameTime();
    public static float Passed => (float)(GetTime() - _offset);

    public static void Reset() => _offset = GetTime();
}