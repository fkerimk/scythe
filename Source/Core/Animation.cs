using Newtonsoft.Json;
using Raylib_cs;

internal unsafe class Animation(Obj obj) : Component(obj) {
    
    public override string LabelIcon => Icons.FaPlayCircle;
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
    
    private ModelAnimation* _rlAnims;

    public override bool Load() {
        
        if (!PathUtil.BestPath($"Models/{Path}.iqm", out var animPath)) return false;
        
        _rlAnims = Raylib.LoadModelAnimations(animPath, ref _count);
        Track = (int)Raymath.Clamp(Track, 0, _count);
        
        return true;
    }

    public override void Loop(bool is2D) {
        
        if (is2D || !IsLoaded || _count == -1 || !IsPlaying || !Obj.Components.TryGetValue("Model", out var model)) return;
        
        _frame = (int)MathF.Floor(_frameRaw);
            
        if (_frame >= _rlAnims[Track].FrameCount) {

            _frame = 0;
            _frameRaw = 0;

            IsPlaying = Looping;
        }
        
        Raylib.UpdateModelAnimation((model as Model)!.RlModel, _rlAnims[Track], _frame);
            
        _frameRaw += Raylib.GetFrameTime() * 30;
    }

    public override void Quit() {
        
        Raylib.UnloadModelAnimations(_rlAnims, _count);
    }
}