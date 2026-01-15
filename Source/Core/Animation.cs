using Newtonsoft.Json;
using Raylib_cs;

// ReSharper disable once ClassNeverInstantiated.Global
internal unsafe class Animation(Obj obj) : ObjType(obj) {
    
    public override int Priority => 1;
    
    public override string LabelIcon => Icons.Animation;
    public override Color LabelColor => Colors.GuiTypeAnimation;

    [RecordHistory] [JsonProperty] [Label("Path")] public string Path { get; set; } = "";
    
    [RecordHistory] [JsonProperty] [Label("Track")] public int Track { get; set {

        if (_count == -1) field = value;
        else if (_count == 0 || value < 0) field = 0;
        else if (value >= _count) field = _count - 1;
        else field = value;
    } }

    [RecordHistory] [JsonProperty] [Label("Is Playing")] public bool IsPlaying { get; set {

        if (!field && value) {

            _frame = 0;
            _frameRaw = 0;
        }

        field = value;
        
    } } = true;

    [RecordHistory] [JsonProperty] [Label("Looping")] public bool Looping { get; set; } = true;

    private int _count = -1;
    private int _frame;
    private float _frameRaw;
    
    private bool _animLoaded;
    private ModelAnimation* _rlAnims;

    public override bool Load(Core core, bool isEditor) {
        
        var path = PathUtil.Relative($"Models/{Path}.iqm");
            
        if (!File.Exists(path)) return false;

        _rlAnims = Raylib.LoadModelAnimations(path, ref _count);
            
        // set loaded
        
        Track = (int)Raymath.Clamp(Track, 0, _count);
        
        return true;
    }

    public override void Loop3D(Core core, bool isEditor) {
        
        if (!IsLoaded || _count == -1 || !IsPlaying || Obj.Parent?.Type is not Model model) return;
        
        _frame = (int)Math.Floor(_frameRaw);
            
        if (_frame >= _rlAnims[Track].FrameCount) {

            _frame = 0;
            _frameRaw = 0;

            IsPlaying = Looping;
        }
        
        Raylib.UpdateModelAnimation(model.RlModel, _rlAnims[Track], _frame);
            
        _frameRaw += Raylib.GetFrameTime() * 60;
    }

    public override void LoopUi(Core core, bool isEditor) {}
    public override void Loop3DEditor(Core core, Viewport viewport) { }
    public override void LoopUiEditor(Core core, Viewport viewport) { }

    public override void Quit() {
        
        Raylib.UnloadModelAnimations(_rlAnims, _count);
    }
}