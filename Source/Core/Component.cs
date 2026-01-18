using System.Numerics;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal class Component(Obj obj, string name) {
    
    [JsonProperty] public string Name => name;
    public readonly Obj Obj = obj;

    public virtual int Priority => 100;
    public virtual string LabelIcon => Icons.Obj;
    public virtual ScytheColor LabelScytheColor => Colors.GuiTypeObject;

    public virtual bool Load() => true;
    public virtual void Loop(bool is2D) {}
    public virtual void Quit() {}

    public bool IsLoaded;
    public bool IsSelected => Obj.IsSelected;

    public Vector3 Up => Obj.Up;
    public Vector3 Fwd => Obj.Fwd;
    public Vector3 Right => Obj.Right;
    public Vector3 FwdFlat => Obj.FwdFlat;
    public Vector3 RightFlat => Obj.RightFlat;
    public Vector3 Pos { get => Obj.Transform.Pos; set => Obj.Transform.Pos = value; }
    public Quaternion Rot { get => Obj.Transform.Rot; set => Obj.Transform.Rot = value; }

    //internal class Comparer : IComparer<Obj> {
    //
    //    public static readonly Comparer Instance = new();
    //    public int Compare(Obj? a, Obj? b) => (a?.Components?.Priority ?? 100).CompareTo(b?.Components?.Priority ?? 100);
    //}
}