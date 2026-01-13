using System.Numerics;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal class Obj {

    public string Icon => Type?.LabelIcon ?? Icons.Obj;
    public Color Color => Type?.LabelColor ?? Colors.GuiTypeObject;
    
    [RecordHistory] [JsonProperty] [Label("Name")] public string Name { get; set; }

    public Obj? Parent;
    [JsonProperty] public readonly List<Obj> Children = [];
    public Matrix4x4 Matrix = Matrix4x4.Identity;
    public Matrix4x4 RotMatrix = Matrix4x4.Identity;
    [JsonProperty] public readonly ObjType? Type;
    public bool IsSelected;
    
    public float3 Right => float3.normalize(Vector3.Transform(new Vector3(1, 0, 0), RotMatrix).to_float3());
    public float3 Up => float3.normalize(Vector3.Transform(new Vector3(0, 1, 0), RotMatrix).to_float3());
    public float3 Fwd => float3.normalize(Vector3.Transform(new Vector3(0, 0, 1), RotMatrix).to_float3());
    
    public Obj(string name, Type? type, Obj? parent = null) {
        
        Name = name;
        Parent = parent;

        if (type == null || type == typeof(Obj)) Type = null;
        else Type = (ObjType?)(Activator.CreateInstance(type, this) ?? Activator.CreateInstance(type));
    }
    
    public void Delete() {

        if (Parent == null) return;
        
        Parent.Children.Remove(this);
        Parent.OrderChildren();
    }

    public void RecordedDelete() {

        var parent = Parent;
        
        History.StartRecording(this, $"Delete {Name}");

        History.ActiveRecord?.UndoAction = () => SetParent(parent);
        History.ActiveRecord?.RedoAction = Delete;
        
        Delete();
        
        History.StopRecording();
    }
    
    public void SetParent(Obj? obj) {
        
        if (obj == null) return;
        if (obj == this) return;
        if (Parent == null) return;
        
        Parent.Children.Remove(this);
        obj.Children.Add(this);
        Parent = obj;

        Parent.OrderChildren();
    }

    private static readonly System.Text.RegularExpressions.Regex NaturalRegex = new(@"(\d+)");

    private void OrderChildren() {
    
        Children.Sort((a, b) => NaturalCompare(a.Name, b.Name));
    }

    private static int NaturalCompare(string a, string b) {
    
        var tokensA = NaturalRegex.Split(a);
        var tokensB = NaturalRegex.Split(b);

        for (var i = 0; i < Math.Min(tokensA.Length, tokensB.Length); i++) {
        
            if (int.TryParse(tokensA[i], out var aNum) && int.TryParse(tokensB[i], out var bNum)) {
            
                var cmp = aNum.CompareTo(bNum);
                if (cmp != 0) return cmp;
            
            } else {
            
                var cmp = string.Compare(tokensA[i], tokensB[i], StringComparison.Ordinal);
                if (cmp != 0) return cmp;
            }
        }

        return tokensA.Length.CompareTo(tokensB.Length);
    }
}