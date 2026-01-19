using System.Numerics;
using Raylib_cs;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal class Obj {

    public string Icon => Icons.Obj;
    public Color Color => Colors.GuiTypeObject;

    [Label("Name"), RecordHistory] public string Name {
        
        get; set {
            
            if (field == value) return;

            if (Parent != null) {
                
                if (Parent.Children.ContainsKey(value)) return; 

                if (!string.IsNullOrEmpty(field))
                    Parent.Children.Remove(field);
                
                Parent.Children.Add(value, this);
            }
            
            field = value;
        }
    } = null!;

    public Obj? Parent; 
    [JsonProperty] public readonly Dictionary<string, Obj> Children = [];
    
    // Components
    [JsonProperty] public Transform Transform = null!;
    
    [JsonProperty] public Dictionary<string, Component> Components { get; set; } = null!;

    // Transform
    public Matrix4x4 Matrix = Matrix4x4.Identity;
    public Matrix4x4 RotMatrix = Matrix4x4.Identity;
    
    public Matrix4x4 WorldMatrix = Matrix4x4.Identity;
    public Matrix4x4 WorldRotMatrix = Matrix4x4.Identity;

    public Vector3 Up    => Vector3.Normalize(new Vector3(WorldRotMatrix.M12, WorldRotMatrix.M22, WorldRotMatrix.M32));
    public Vector3 Fwd   => Vector3.Normalize(new Vector3(WorldRotMatrix.M13, WorldRotMatrix.M23, WorldRotMatrix.M33));
    public Vector3 Right => Vector3.Normalize(new Vector3(WorldRotMatrix.M11, WorldRotMatrix.M21, WorldRotMatrix.M31));
    public Vector3 FwdFlat { get { var fwd = Fwd; fwd.Y = 0; fwd = Vector3.Normalize(fwd); return fwd;  } }
    public Vector3 RightFlat { get { var right = Right; right.Y = 0; right = Vector3.Normalize(right); return right;  } }
    public Vector3 Pos { get => Transform.Pos; set => Transform.Pos = value; }
    public Quaternion Rot { get => Transform.Rot; set => Transform.Rot = value; }
    
    public bool IsSelected;
    
    public Obj(string? name, Obj? parent) {
        
        if (name == null) return;
        
        Parent = parent;
        Name = name;
        
        // Components
        Transform = new Transform(this);
        Components = new Dictionary<string, Component>();
    }

    public void Delete() {

        if (Parent == null) return;
        
        Parent.Children.Remove(Name);
        Parent.OrderChildren();
    }

    public void RecordedDelete() {

        var parent = Parent;
        
        History.StartRecording(this, $"Delete {Name}");

        History.SetUndoAction(() => SetParent(parent));
        History.SetRedoAction(Delete);
        
        Delete();
        
        History.StopRecording();
    }
    
    [MoonSharpHidden]
    public void SetParent(Obj? obj) {
        
        if (obj == null || obj == this || Parent == null) return;

        Parent.Children.Remove(Name);
        obj.Children.Add(Name, this);
        Parent = obj;
        
        Parent.OrderChildren();
    }

    private static readonly System.Text.RegularExpressions.Regex NaturalRegex = new(@"(\d+)");

    private void OrderChildren() {
    
        //if (CommandLine.Editor)
        //     Children.Sort((a, b) => NaturalCompare(a.Name, b.Name));
        //else Children.Sort(Component.Comparer.Instance);
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
    
    public unsafe void DecomposeWorldMatrix(out Vector3 worldPos, out Quaternion worldRot, out Vector3 worldScale) {
        
        var position = Vector3.Zero;
        var rotation = Quaternion.Identity;
        var lossyScale = Vector3.One;
    
        Raymath.MatrixDecompose(WorldMatrix, &position, &rotation, &lossyScale);
        
        worldPos = position;
        worldScale = lossyScale;
        worldRot = rotation;
    }
    
    [MoonSharpHidden]
    public Obj? Find(params string[] names) {
        
        if (names.Length == 0) return this;
        
        var current = this;

        foreach (var name in names)
            current = current.Children[name];

        return current.Name != names[^1] ? null : current;
    }
    
    [MoonSharpHidden]
    public Component? FindComponent(params string[] names) => Find(names[..^1])?.Components[names[^1]];
    
    public Obj? Find(Table t) => Find(t.Values.Select(v => v.String).ToArray());
    public Component? FindComponent(Table t) => FindComponent(t.Values.Select(v => v.String).ToArray());

    public Component MakeComponent(string name) {
        
        if (Components.ContainsKey(name)) throw new TypeLoadException();
        var component = Activator.CreateInstance(Type.GetType(name) ?? throw new KeyNotFoundException(), this) as Component ?? throw new InvalidOperationException();
        Components[name] = component;
        return component;
    }
    
    public string SafeNameForChild(string name) {
        
        var newName = name;
        
        var i = 0;

        while (Children.ContainsKey(newName)) {
            
            i++;
            newName = name + i;
        }
        
        return newName;
    }

    public bool TryGetComponent<T>(out T component) where T : Component {
        
        component = (Components.Values.FirstOrDefault(c => c is T) as T)!;
        return true;
    }
}

internal static partial class Extensions {
    
    public static IEnumerable<Obj> GetChildrenRecursive (this Obj obj) {

        foreach (var child in obj.Children) {
            
            yield return child.Value;

            foreach (var grandChild in child.Value.GetChildrenRecursive()) {
                
                yield return grandChild;
            }
        }
    }
}