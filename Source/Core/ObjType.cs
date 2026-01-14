using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal abstract class ObjType(Obj obj) {

    public virtual int Priority => 100;
    
    [JsonProperty] public string Name => GetType().Name;
    
    protected readonly Obj Obj = obj;

    public virtual string LabelIcon => Icons.Obj;
    public virtual Color LabelColor => Colors.GuiTypeObject;
    
    public abstract void Loop3D(Core core, bool isEditor);
    public abstract void LoopUi(Core core, bool isEditor);
    public abstract void Loop3DEditor(Core core, Viewport viewport);
    public abstract void LoopUiEditor(Core core, Viewport viewport);
    public abstract void Quit();

    public bool IsSelected => (Obj.Parent != null && (Obj.Parent.IsSelected || Obj.IsSelected ||  Obj.Parent.Children.Any(obj => obj.IsSelected)));
    
    internal class Comparer : IComparer<Obj> {
    
        public static readonly Comparer Instance = new();
        public int Compare(Obj? a, Obj? b) => (a?.Type?.Priority ?? 100).CompareTo(b?.Type?.Priority ?? 100);
    }
}