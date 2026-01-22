internal abstract class Asset {
    
    public bool IsLoaded { get; protected set; }
    public string File { get; internal set; } = "";

    public virtual bool Load() =>  true;
    public virtual void Unload() { }
}