using System.Numerics;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Raylib_cs;

[JsonObject(MemberSerialization.OptIn)]
internal class Obj {

    public Obj() { Name = ""; }
    
    public string Icon => Type?.LabelIcon ?? Icons.Obj;
    public ScytheColor ScytheColor => Type?.LabelScytheColor ?? Colors.GuiTypeObject;
    
    [RecordHistory] [JsonProperty] [Label("Name")] public string Name { get; set; }

    public Obj? Parent; 
    [JsonProperty] public readonly List<Obj> Children = [];
    
    [JsonProperty] public ObjType? Type { get; set; }
    
    public Matrix4x4 Matrix = Matrix4x4.Identity;
    public Matrix4x4 RotMatrix = Matrix4x4.Identity;
    
    public Matrix4x4 WorldMatrix = Matrix4x4.Identity;
    public Matrix4x4 WorldRotMatrix = Matrix4x4.Identity;
    
    public Vector3 Right => Raymath.Vector3Normalize(Vector3.Transform(Vector3.UnitX, WorldRotMatrix));
    public Vector3 Up => Raymath.Vector3Normalize(Vector3.Transform(Vector3.UnitY, WorldRotMatrix));
    public Vector3 Fwd => Raymath.Vector3Normalize(Vector3.Transform(Vector3.UnitZ, WorldRotMatrix));
    
    public bool IsSelected;
    
    public Obj(string name, Type? type, Obj? parent) {
        
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
    
    [MoonSharpHidden]
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
    
        if (CommandLine.Editor)
             Children.Sort((a, b) => NaturalCompare(a.Name, b.Name));
        else Children.Sort(ObjType.Comparer.Instance);
    }
    
    private static int NaturalCompare(string a, string b) {
    
        var tokensA = NaturalRegex.Split(a);
        var tokensB = NaturalRegex.Split(b);

        for (var i = 0; i < MathF.Min(tokensA.Length, tokensB.Length); i++) {
        
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

    public unsafe void DecomposeMatrix(out Vector3 pos, out Quaternion rot, out Vector3 scale) {
        
        var position = Vector3.Zero;
        var rotation = Quaternion.Identity;
        var lossyScale = Vector3.One;
    
        Raymath.MatrixDecompose(Matrix, &position, &rotation, &lossyScale);
        
        pos = position;
        rot = rotation;
        scale = lossyScale;
    }

    public T? FindType<T>() where T : ObjType => (from child in this.GetChildrenRecursive() where child.Type is T select child.Type).FirstOrDefault() as T;
    public ObjType? FindType(string name) => (from child in this.GetChildrenRecursive() where child.Type?.Name == name select child.Type).FirstOrDefault();
    public ObjType? FindParentType(string name) => (from child in Parent?.GetChildrenRecursive() where child.Type?.Name == name select child.Type).FirstOrDefault();
}

internal static partial class Extensions {
    
    public static IEnumerable<Obj> GetChildrenRecursive (this Obj obj) {

        foreach (var child in obj.Children) {
            
            yield return child;

            foreach (var grandChild in child.GetChildrenRecursive()) {
                
                yield return grandChild;
            }
        }
    }
}