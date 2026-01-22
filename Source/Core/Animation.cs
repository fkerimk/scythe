using Newtonsoft.Json;
using Raylib_cs;

internal unsafe class Animation(Obj obj) : Component(obj) {
    
    public override string LabelIcon => Icons.FaPlayCircle;
    public override Color LabelColor => Colors.GuiTypeAnimation;

    [RecordHistory] [JsonProperty] [Label("Path")] [FilePath("Models", ".iqm")] public string Path { get; set; } = "";
    
    [RecordHistory] [JsonProperty] [Label("Track")] public int Track { get; set {

        if (!IsLoaded) return;
        if (Asset.AnimationCount == -1) field = value;
        else if (Asset.AnimationCount == 0 || value < 0) field = 0;
        else if (value >= Asset.AnimationCount) field = Asset.AnimationCount - 1;
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

    private int _frame;
    private float _frameRaw;
    
    private AnimationAsset Asset = null!;

    public override bool Load() {
        
        if (!PathUtil.BestPath($"Models/{Path}.iqm", out var animationPath)) return false;
        if (!AssetManager.Load(animationPath, out Asset!)) return false;
        
        Track = (int)Raymath.Clamp(Track, 0, Asset.AnimationCount);
        
        return true;
    }

    public override void Logic() {
        
        if (Asset.AnimationCount == -1 || !IsPlaying || !Obj.Components.TryGetValue("Model", out var component) || component is not Model { IsLoaded: true } model) return;
        
        _frame = (int)MathF.Floor(_frameRaw);
            
        if (_frame >= Asset.Animations[Track].FrameCount) {

            _frame = 0;
            _frameRaw = 0;

            IsPlaying = Looping;
        }
        
        Raylib.UpdateModelAnimation(model.RlModel, Asset.Animations[Track], _frame);
            
        _frameRaw += Raylib.GetFrameTime() * 60;
    }
}