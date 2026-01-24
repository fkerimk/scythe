using System.Numerics;
using Newtonsoft.Json;
using Raylib_cs;

internal class Animation(Obj obj) : Component(obj) {
    
    public override string LabelIcon => Icons.FaPlayCircle;
    public override Color LabelColor => Colors.GuiTypeAnimation;

    [RecordHistory] [JsonProperty] [Label("Path")] [FindAsset("AnimationAsset")] public string Path { get; set; } = "";
    
    [RecordHistory] [JsonProperty] [Label("Track")] public int Track { get; set {

        if (!IsLoaded) return;
        
        if (_asset.Animations.Count == 0 || value < 0) field = 0;
        
        else if (value >= _asset.Animations.Count) field = _asset.Animations.Count - 1;
        else field = value;
    } }

    [RecordHistory] [JsonProperty] [Label("Is Playing")] public bool IsPlaying { get; set {

        if (!field && value)
            _frameRaw = 0;

        field = value;
        
    } } = true;

    [RecordHistory] [JsonProperty] [Label("Looping")] public bool Looping { get; set; } = true;

    private float _frameRaw;
    
    private AnimationAsset _asset = null!;
    private Dictionary<string, AnimationChannel>? _channelMap;
    private int _lastTrack = -1;

    public override bool Load() {
        
        var loaded = AssetManager.Get<AnimationAsset>(Path);
        if (loaded is not { IsLoaded: true }) return false;
        
        _asset = loaded;
        Track = (int)Raymath.Clamp(Track, 0, _asset.Animations.Count - 1);
        
        return true;
    }

    public override void Logic() {
        
        if (_asset is not { IsLoaded: true } || _asset.Animations.Count == 0) return;
        if (!IsPlaying || !Obj.Components.TryGetValue("Model", out var component) || component is not Model { IsLoaded: true } model) return;
        
        var modelAsset = model.AssetRef;
        
        if (!modelAsset.IsLoaded) return;

        if (Track != _lastTrack) {
            
            _channelMap = _asset.Animations[Track].Channels.ToDictionary(c => c.NodeName);
            _lastTrack = Track;
        }

        var clip = _asset.Animations[Track];
        
        _frameRaw += Raylib.GetFrameTime() * (float)clip.TicksPerSecond;
        
        if (_frameRaw >= clip.Duration) {
            
            if (Looping) _frameRaw = (float)(_frameRaw % clip.Duration);
            
            else {
                
                _frameRaw = (float)clip.Duration;
                IsPlaying = false;
            }
        }

        AssimpLoader.UpdateAnimation(modelAsset.RootNode, clip, _frameRaw, Matrix4x4.Identity, modelAsset.GlobalInverse, model.Bones, _channelMap!);

        foreach (var mesh in model.Meshes)
            AssimpLoader.SkinMesh(mesh, model.Bones);
    }
}