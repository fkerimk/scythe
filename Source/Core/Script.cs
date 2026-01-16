using System.Numerics;
using Raylib_cs;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

internal class Script(Obj obj) : ObjType(obj) {
    
    public override int Priority => 3;
    [RecordHistory] [JsonProperty] [Label("Path")] public string Path { get; set; } = "";

    public required MoonSharp.Interpreter.Script LuaScript;
    public DynValue? LuaLoop;

    public static LuaMt? LuaMt;
    public static LuaTime? LuaTime;
    public static LuaKb? LuaKb;
    public static LuaMouse? LuaMouse;
    public static LuaF2? LuaF2;
    public static LuaF3? LuaF3;
    public static LuaQuat? LuaQuat;
    
    public static void Register() {
        
        UserData.RegisterType<LuaMt>(); LuaMt = new LuaMt();
        UserData.RegisterType<LuaTime>(); LuaTime = new LuaTime();
        UserData.RegisterType<LuaKb>(); LuaKb = new LuaKb();
        UserData.RegisterType<LuaMouse>(); LuaMouse = new LuaMouse();
        UserData.RegisterType<LuaF2>(); LuaF2 = new LuaF2();
        UserData.RegisterType<LuaF3>(); LuaF3 = new LuaF3();
        UserData.RegisterType<LuaQuat>(); LuaQuat = new LuaQuat();
        
        UserData.RegisterType<Vector2>();
        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Quaternion>();
        
        UserData.RegisterType<Level>();
        
        UserData.RegisterType<Obj>();
        UserData.RegisterType<ObjType>();
        UserData.RegisterType<Animation>();
        UserData.RegisterType<Camera>();
        UserData.RegisterType<Light>();
        UserData.RegisterType<Model>();
        UserData.RegisterType<Script>();
        UserData.RegisterType<Transform>();

        Make(generateDefinitions: true);
    }

    private static MoonSharp.Interpreter.Script Make(Core? core = null, Obj? obj = null, bool generateDefinitions = false) {

        var dummyObj = new Obj();
        var dummyCam = new Camera { Cam = null! };
        var dummyLvl = new Level();
        
        var script = new MoonSharp.Interpreter.Script {

            Options = {

                DebugPrint = Console.WriteLine
            },
            
            Globals = {
                
                ["obj"] = obj ?? dummyObj,
                ["level"] = core?.ActiveLevel ?? dummyLvl,
                ["cam"] = core?.ActiveLevel?.FindType<Camera>() ?? dummyCam,
                ["f2"] = LuaF2,
                ["f3"] = LuaF3,
                ["mt"] = LuaMt,
                ["time"] = LuaTime,
                ["kb"] = LuaKb,
                ["mouse"] = LuaMouse,
                ["quat"] = LuaQuat,
            }
        };

        if (!generateDefinitions) return script;
        
        if (PathUtil.BestPath("Resources/definitions.lua", out var definitionsPath))
            LuaDefinitionGenerator.Generate(script, definitionsPath);

        return script;
    }
    
    public override bool Load(Core core, bool isEditor) {

        if (isEditor) return false;
        if (!PathUtil.BestPath($"Scripts/{Path}.lua", out var scriptPath)) return false;

        LuaScript = Make(core, Obj);

        var code = File.ReadAllText(scriptPath);
        
        LuaScript.DoString(code);
        LuaLoop = LuaScript.Globals.Get("loop");

        return true;
    }

    public override void Loop3D(Core core, bool isEditor) {
        
        if (isEditor || !IsLoaded) return;
        if (LuaLoop == null || LuaLoop.IsNil()) return;
        
        LuaScript.Call(LuaLoop, Raylib.GetFrameTime());
    }
}