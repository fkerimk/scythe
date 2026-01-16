using System.Reflection;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class Level {

    public readonly string Name;
    public readonly string JsonPath;
    public readonly Obj Root;
    public readonly Core Core;
    
    public Level(string name, Core core) {

        Name = name;
        JsonPath = $"Levels/{Name}.json";
        Root = Load();
        Core = core;
    }

    private Obj Load() {
        
        var root = new Obj("Root", null, null);

        var jsonText = File.ReadAllText(PathUtil.Relative(JsonPath));
        var rawData = JObject.Parse(jsonText);

        if (rawData["Children"] is not JArray children) return root;
        
        foreach (var childData in children)
            BuildHierarchy((JObject)childData, root);

        return root;
    }

    public void Save() {
        
        var settings = new JsonSerializerSettings { 
            
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None 
        };

        Formatting formatting;

        if (!Enum.TryParse(Config.Level.Formatting, out formatting))
            formatting = Formatting.None;
        
        var json = JsonConvert.SerializeObject(Root,formatting, settings);

        File.WriteAllText(JsonPath, json);
    }
    
    private void BuildHierarchy(JObject data, Obj parent) {
        
        var name = data["Name"]?.ToString() ?? "Object";
        
        var typeStr = data["Type"]?.Type == JTokenType.Object 
            ? data["Type"]?["Name"]?.ToString() 
            : null;

        var currentObj = BuildObject(name, parent, typeStr);

        if (currentObj.Type != null && data["Type"] != null)
            JsonConvert.PopulateObject(data["Type"]!.ToString(), currentObj.Type);

        if (data["Children"] is not JArray children) return;
        
        foreach (var childData in children)
            BuildHierarchy((JObject)childData, currentObj);
    }
    
    public Obj BuildObject(string name, Obj? parent, string? type) {

        var typeClass = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
        
        var obj = new Obj(name, typeClass, parent);
        obj.SetParent(parent);

        return obj;
    }

    public Obj RecordedBuildObject(string name, Obj? parent, string? type) {
        
        History.StartRecording(parent!,  $"Create {name}");
        
        var obj = BuildObject(name, parent, type);

        History.ActiveRecord?.UndoAction = () => obj.Delete();
        History.ActiveRecord?.RedoAction = () => obj.SetParent(parent);
        
        History.StopRecording();
        return obj;
    }

    public Obj CloneObject(Obj source) {
        
        var typeName = source.Type?.GetType().Name;
    
        var clone = BuildObject(source.Name + "_Clone", source.Parent, typeName);

        if (source.Type != null && clone.Type != null) {
            
            var data = JsonConvert.SerializeObject(source.Type);
            JsonConvert.PopulateObject(data, clone.Type);
        }

        foreach (var childClone in source.Children.ToList().Select(CloneObject))
            childClone.SetParent(clone);

        return clone;
    }

    public void RecordedCloneObject(Obj source) {
        
        History.StartRecording(source.Parent!, $"Duplicate {source.Name}");

        var clone = CloneObject(source);
        var parent = source.Parent!;

        History.ActiveRecord?.UndoAction = () => clone.Delete();
        History.ActiveRecord?.RedoAction = () => clone.SetParent(parent);
        
        History.StopRecording();
    }
    
    public T? FindType<T>() where T : ObjType => (from obj in Root.GetChildrenRecursive() where obj.Type is T select obj.Type).FirstOrDefault() as T;
    public ObjType? FindType(string name) => (from child in Root.GetChildrenRecursive() where child.Type?.Name == name select child.Type).FirstOrDefault();
}