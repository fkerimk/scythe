using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonObject(MemberSerialization.OptIn)]
internal class Level {
    
    private readonly string _name = null!;
    private string _jsonPath = null!;
    
    [JsonProperty] public readonly Obj Root = null!;

    public Level(string? name) {

        if (name == null) return;
        
        _name = name;
        
        if (!PathUtil.BestPath($"Levels/{_name}.json", out _jsonPath))
            throw new FileNotFoundException($"Could not find level json file {_jsonPath}");

        Root = new Obj("Root", null);

        var jsonText = File.ReadAllText(_jsonPath);
        var rawData = JObject.Parse(jsonText);

        if (rawData["Root"]?["Children"] is not JObject children) return;
        
        foreach (var property in children.Properties())
            BuildHierarchy(new KeyValuePair<string, JToken>(property.Name, property.Value), Root);
    }

    public void Save() {
        
        var settings = new JsonSerializerSettings { 
            
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None 
        };

        if (!Enum.TryParse(Config.Level.Formatting, out Formatting formatting))
            formatting = Formatting.None;
        
        var json = JsonConvert.SerializeObject(this, formatting, settings);

        File.WriteAllText(_jsonPath, json);
    }
    
    private static void BuildHierarchy(KeyValuePair<string, JToken> dataPair, Obj parent) {
        
        if (dataPair.Value is not JObject data) return;
        
        var name = dataPair.Key;
        var obj = MakeObject(name, parent);
        
        // Load transform
        if (data["Transform"] is JObject)
            JsonConvert.PopulateObject(data["Transform"]!.ToString(), obj.Transform);
        
        // Load components
        var components = new Dictionary<string, Component>();

        if (data["Components"] is JObject jsonComponents) {

            foreach (var property in jsonComponents.Properties()) {
                
                if (Activator.CreateInstance(Type.GetType(property.Name) ?? throw new KeyNotFoundException($"{property.Name} cant be found"), obj) is not Component component) continue;
                JsonConvert.PopulateObject(data["Components"]![property.Name]!.ToString(), component);
                components[property.Name] = component;
            }
        }

        obj.Components = components;

        if (data["Children"] is not JObject children) return;

        foreach (var property in children.Properties())
            BuildHierarchy(new KeyValuePair<string, JToken>(property.Name, property.Value), obj);
    }
    
    public static Obj MakeObject(string name, Obj? parent) {

        var obj = new Obj(parent == null ? name : parent.SafeNameForChild(name), parent);
        obj.SetParent(parent);

        return obj;
    }

    public Obj RecordedBuildObject(string name, Obj? parent) {
        
        History.StartRecording(parent!,  $"Create {name}");
        
        var obj = MakeObject(name, parent);

        History.SetUndoAction(obj.Delete);
        History.SetRedoAction(() => obj.SetParent(parent));
        
        History.StopRecording();
        return obj;
    }

    private Obj CloneObject(Obj source, Obj? newParent = null) {
        
        //var name = (newParent == null) ? source.Name + "_Clone" : source.Name;
        
        var parentToUse = newParent ?? source.Parent;

        var name = source.Name;
        
        if (newParent == null && parentToUse != null)
            name = parentToUse.SafeNameForChild(name);

        var clone = new Obj(name, parentToUse);

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
        foreach (var child in source.Children.Values.ToList())
            CloneObject(child, clone);

        return clone;
    }

    public Obj RecordedCloneObject(Obj source) {
        
        History.StartRecording(source.Parent!, $"Duplicate {source.Name}");

        var clone = CloneObject(source);
        var parent = source.Parent!;

        History.SetUndoAction(clone.Delete);
        History.SetRedoAction(() => clone.SetParent(parent));
        
        History.StopRecording();
        return clone;
    }

    [MoonSharpHidden] public Obj? Find(string[] names) => Root.Find(names);
    [MoonSharpHidden] public Component? FindComponent(string[] names) => Root.FindComponent(names);
    
    public Obj? Find(Table t) => Root.Find(t.Values.Select(v => v.String).ToArray());
    public Component? FindComponent(Table t) => Root.FindComponent(t.Values.Select(v => v.String).ToArray());
}