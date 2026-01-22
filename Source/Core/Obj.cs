using System.Numerics;
using Raylib_cs;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
internal class Obj {

    public static string Icon => Icons.FaDotCircleO;
    public static Color Color => Colors.GuiTypeObject;

    public static event Action<Obj>? OnDelete;

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
        
        OnDelete?.Invoke(this);
        Dispose();
        Parent.Children.Remove(Name);
    }

    public void Dispose() {
        
        IsSelected = false;
        
        Transform.UnloadAndQuit();

        foreach (var component in Components.Values)
            component.UnloadAndQuit();

        foreach (var child in Children.Values)
            child.Dispose();
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
    public void SetParent(Obj? obj, bool keepWorld = false) {
        
        if (obj == null || obj == this || Parent == null) return;

        Vector3 wp = Vector3.Zero;
        Quaternion wr = Quaternion.Identity;
        Vector3 ws = Vector3.One;

        if (keepWorld) DecomposeWorldMatrix(out wp, out wr, out ws);

        Parent.Children.Remove(Name);
        obj.Children.Add(Name, this);
        Parent = obj;

        if (keepWorld) {
            
            Transform.WorldPos = wp;
            Transform.WorldRot = wr;
            Transform.WorldScale = ws;
        }
    }

    public void RecordedSetParent(Obj? obj) {

        if (obj == null || obj == this || Parent == null) return;
        
        var oldParent = Parent;
        History.StartRecording(this, $"Change Parent of {Name}");
        History.StartRecording(Transform);
        
        SetParent(obj, true);

        History.SetUndoAction(() => SetParent(oldParent));
        History.SetRedoAction(() => SetParent(obj, true));
        
        History.StopRecording();
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

    public string[] GetPathFromRoot() {
        var path = new List<string>();
        var current = this;
        while (current != null && current.Parent != null) {
            path.Add(current.Name);
            current = current.Parent;
        }
        path.Reverse();
        return path.ToArray();
    }
    
    [MoonSharpHidden]
    public Obj? Find(params string[] names) {
        
        if (names.Length == 0) return this;
        
        var current = this;

        current = names.Aggregate(current, (current1, name) => current1.Children[name]);

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
}

internal static partial class Extensions {
    
    extension(Obj source) {
        
        private Obj Clone(Obj? parent = null) {
        
            parent ??= source.Parent;

            var name = source.Name;
        
            if (parent != null)
                name = Generators.AvailableName(name, parent.Children.Keys);

            var clone = new Obj(name, parent);

            // Copy Transform
            var transformJson = JsonConvert.SerializeObject(source.Transform);
            JsonConvert.PopulateObject(transformJson, clone.Transform);

            // Copy Components
            foreach (var (key, sourceComponent) in source.Components) {
            
                var compType = sourceComponent.GetType();
                
                if (Activator.CreateInstance(compType, clone) is not Component cloneComp) continue;
                
                var compJson = JsonConvert.SerializeObject(sourceComponent);
                JsonConvert.PopulateObject(compJson, cloneComp);
                
                clone.Components[key] = cloneComp;
            }

            // Clone children recursively
            foreach (var child in source.Children.Values.ToList()) child.Clone(clone);

            return clone;
        }

        public Obj CloneRecorded() {
        
            History.StartRecording(source.Parent!, $"Duplicate {source.Name}");

            var clone = source.Clone();
            var parent = source.Parent!;

            History.SetUndoAction(clone.Delete);
            History.SetRedoAction(() => clone.SetParent(parent));
        
            History.StopRecording();
            return clone;
        }
    }
}