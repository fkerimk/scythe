using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal class ObjType(Obj obj) {
    
    [JsonProperty] public string Name => GetType().Name;
    public readonly Obj Obj = obj;

    public virtual int Priority => 100;
    public virtual string LabelIcon => Icons.Obj;
    public virtual Color LabelColor => Colors.GuiTypeObject;

    public virtual bool Load(Core core, bool isEditor) => true;
    public virtual void Loop3D(Core core, bool isEditor) {}
    public virtual void LoopUi(Core core, bool isEditor) {}
    public virtual void Loop3DEditor(Core core, Viewport viewport) {}
    public virtual void LoopUiEditor(Core core, Viewport viewport) {}
    public virtual void Quit() {}

    public bool IsLoaded;
    public bool IsSelected => (Obj.Parent != null && (Obj.Parent.IsSelected || Obj.IsSelected ||  Obj.Parent.Children.Any(obj => obj.IsSelected)));
    
    public ObjType? FindType(string name) => (from child in Obj.GetChildrenRecursive() where child.Type?.Name == name select child.Type).FirstOrDefault();
    public ObjType? FindParentType(string name) => (from child in Obj.Parent?.GetChildrenRecursive() where child.Type?.Name == name select child.Type).FirstOrDefault();
    
    internal class Comparer : IComparer<Obj> {
    
        public static readonly Comparer Instance = new();
        public int Compare(Obj? a, Obj? b) => (a?.Type?.Priority ?? 100).CompareTo(b?.Type?.Priority ?? 100);
    }
}