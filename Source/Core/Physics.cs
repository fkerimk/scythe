using System;
using System.Numerics;
using Jitter2;
using static Raylib_cs.Raylib;

internal static class Physics {
    
    public static World World { get; private set; } = null!;

    public static void Init() {
        World = new World();
        World.Gravity = new Vector3(0, -9.81f, 0);
    }
    
    public static void Update() {
        
        var dt = GetFrameTime();
        if (dt <= 0) return;
        
        World.Step(dt);
    }
}
