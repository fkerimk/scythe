internal abstract class Asset {
    
    public bool IsLoaded { get; protected set; }
    public string File { get; internal init; } = "";
    public Raylib_cs.Texture2D? Thumbnail { get; internal set; }

    public virtual bool Load() =>  true;
    public virtual void Unload() { }
}