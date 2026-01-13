using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class Level {

    public readonly string Name;
    public readonly Obj Root;

    public string JsonPath => $"Levels/{Name}.json";
    
    public Level(string name) {

        Name = name;
        Root = Load();
    }

    private Obj Load() {
        
        var root = new Obj("Root", null);

        var jsonText = File.ReadAllText(PathUtil.Relative(JsonPath));
        var rawData = JObject.Parse(jsonText);

        if (rawData["c"] is not JArray children) return root;
        
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
            
        var json = JsonConvert.SerializeObject(Root, Formatting.None, settings);

        File.WriteAllText(JsonPath, json);
    }
    
    private static void BuildHierarchy(JObject data, Obj parent) {
        
        var name = data["n"]?.ToString() ?? "Object";
        
        var typeStr = data["t"]?.Type == JTokenType.Object 
            ? data["t"]?["n"]?.ToString() 
            : null;

        var currentObj = BuildObject(name, parent, typeStr);

        if (currentObj.Type != null && data["t"] != null)
            JsonConvert.PopulateObject(data["t"]!.ToString(), currentObj.Type);

        if (data["c"] is not JArray children) return;
        
        foreach (var childData in children)
            BuildHierarchy((JObject)childData, currentObj);
    }
    
    public static Obj BuildObject(string name, Obj parent, string? type = null) {

        var typeClass = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
        
        var obj = new Obj(name, typeClass, parent);
        obj.set_parent(parent);

        return obj;
    }
}