using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal abstract class ObjType(Obj obj) {
    
    [JsonProperty] public string Name => GetType().Name;
    
    protected readonly Obj Obj = obj;

    public virtual string LabelIcon => Icons.Obj;
    public virtual Color LabelColor => Colors.GuiTypeObject;
    
    public abstract void Loop3D(bool isEditor);
    public abstract void LoopUi(bool isEditor);
    public abstract void Loop3DEditor(Viewport viewport);
    public abstract void LoopUiEditor(Viewport viewport);
    public abstract void Quit();

    public bool IsSelected => (Obj.Parent != null && (Obj.Parent.IsSelected || Obj.IsSelected ||  Obj.Parent.Children.Any(obj => obj.IsSelected)));
}