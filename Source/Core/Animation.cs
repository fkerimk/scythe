using Newtonsoft.Json;
using Raylib_cs;

// ReSharper disable once ClassNeverInstantiated.Global
internal unsafe class Animation(Obj obj) : ObjType(obj) {
    
    public override string LabelIcon => Icons.Animation;
    public override Color LabelColor => Colors.GuiTypeAnimation;

    [JsonProperty] [Label("Path")] public string Path { get; set; } = "";
    
    [JsonProperty] [Label("Track")] public int Track { get; set {

        if (_count == -1) field = value;
        else if (_count == 0 || value < 0) field = 0;
        else if (value >= _count) field = _count - 1;
        else field = value;
    } }

    [JsonProperty] [Label("Is Playing")] public bool IsPlaying { get; set {

        if (!field && value) {

            _frame = 0;
            _frameRaw = 0;
        }

        field = value;
        
    } } = true;

    [JsonProperty] [Label("Looping")] public bool Looping { get; set; } = true;

    private int _count = -1;
    private int _frame;
    private float _frameRaw;
    
    private bool _animLoaded;
    private ModelAnimation* _rlAnims;
    
    public override void Loop3D(bool isEditor) {
        
        if (!_animLoaded) {

            _rlAnims = Raylib.LoadModelAnimations(PathUtil.Relative($"Models/{Path}.glb"), ref _count);
            _animLoaded = true;
        }

        else if (IsPlaying && Obj.Parent?.Type is Model model) {
                
            _frame = (int)Math.Floor(_frameRaw);
            
            if (_frame >= _rlAnims[Track].FrameCount) {

                _frame = 0;
                _frameRaw = 0;

                IsPlaying = Looping;
            }
        
            Raylib.UpdateModelAnimation(model.RlModel, _rlAnims[Track], _frame);
            
            _frameRaw += Raylib.GetFrameTime() * 60;
        }
    }

    public override void LoopUi(bool isEditor) {}
    public override void Loop3DEditor(Viewport viewport) { }
    public override void LoopUiEditor(Viewport viewport) { }

    public override void Quit() {
        
        Raylib.UnloadModelAnimations(_rlAnims, _count);
    }
}