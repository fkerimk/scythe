using System.Numerics;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonObject(MemberSerialization.OptIn)]
internal class Level {
    
    // Custom converter to handle path relativization
    public class RelativePathConverter : JsonConverter {
        
        public override bool CanConvert(Type objectType) => objectType == typeof(string);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            
            var val = (string?)reader.Value;
            if (string.IsNullOrEmpty(val)) return val;
            
            // Try explicit mod path first
            if (PathUtil.BestPath(val, out var fullPath)) return fullPath;
            
            // Try asset lookup
            if (!Path.IsPathRooted(val)) {
                // If it's relative, assume it's relative to Mod Root or Resources
                if (PathUtil.BestPath(val, out var bestPath)) return bestPath;
            }
            
            return val; 
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            
            var val = (string?)value;
            
            if (string.IsNullOrEmpty(val)) {
                
                writer.WriteValue(val);
                return;
            }

            var modPath = Config.Mod.Path;
            var resPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            
            // Convert to relative if possible
            if (val.StartsWith(modPath, StringComparison.OrdinalIgnoreCase))
                val = Path.GetRelativePath(modPath, val).Replace('\\', '/');
            
            else if (val.StartsWith(resPath, StringComparison.OrdinalIgnoreCase))
                val = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, val).Replace('\\', '/');
            
            writer.WriteValue(val);
        }
    }
    
    public string Name { get; set; } = null!;
    public string JsonPath { get; set; } = null!;
    public bool IsDirty { get; set; }
    
    [JsonProperty] public readonly Obj Root = null!;
    [JsonProperty] public CameraData? EditorCamera;

    public class CameraData {
        
        public Vector3 Position;
        public Vector2 Rotation;
    }

    public Level(string? name) {

        if (name == null) return;
        
        Name = name;
        
        if (!PathUtil.BestPath($"Levels/{Name}.level.json", out var path))
            if (!PathUtil.BestPath($"{Name}.level.json", out path))
                if (!PathUtil.BestPath($"Levels/{Name}.json", out path))
                    if (!PathUtil.BestPath($"{Name}.json", out path))
                        throw new FileNotFoundException($"Could not find level file {Name} with .level.json or .json extension");

        JsonPath = path;
        Root = new Obj("Root", null);

        LoadInternal();
    }

    public Level(string name, string path, bool load = true) {

        Name = name;
        JsonPath = path;
        Root = new Obj("Root", null);

        if (load) LoadInternal();
    }

    private void LoadInternal() {

        SafeExec.Try(() => {
            
            var jsonText = File.ReadAllText(JsonPath);
            var rawData = JObject.Parse(jsonText);

            if (rawData["Root"]?["Children"] is JObject children) {
                
                foreach (var property in children.Properties())
                    BuildHierarchy(new KeyValuePair<string, JToken>(property.Name, property.Value), Root);
            }

            // Load camera
            if (CommandLine.Editor && rawData["EditorCamera"] is JObject cameraJson) {
                
                EditorCamera = cameraJson.ToObject<CameraData>();
                
                if (EditorCamera != null) {
                    
                    FreeCam.Pos = EditorCamera.Position;
                    FreeCam.Rot = EditorCamera.Rotation;
                }
            }
        });
    }

    public void Save() {
        
        if (CommandLine.Editor) {
            
            EditorCamera = new CameraData {
                Position = FreeCam.Pos,
                Rotation = FreeCam.Rot
            };
        }
        
        var settings = new JsonSerializerSettings { 
            
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            Converters = { new RelativePathConverter() }
        };

        if (!Enum.TryParse(Config.Level.Formatting, out Formatting formatting))
            formatting = Formatting.None;
        
        var json = JsonConvert.SerializeObject(this, formatting, settings);

        File.WriteAllText(JsonPath, json);
        IsDirty = false;
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

        var obj = new Obj(parent == null ? name : Generators.AvailableName(name, parent.Children.Keys), parent);
        obj.SetParent(parent);

        return obj;
    }

    public static Obj RecordedMakeObject(string name, Obj? parent) {
        
        History.StartRecording(parent!,  $"Create {name}");
        
        var obj = MakeObject(name, parent);

        History.SetUndoAction(obj.Delete);
        History.SetRedoAction(() => obj.SetParent(parent));
        
        if (Core.ActiveLevel != null) Core.ActiveLevel.IsDirty = true;
        History.StopRecording();
        return obj;
    }

    [MoonSharpHidden] public Obj? Find(string[] names) => Root.Find(names);
    [MoonSharpHidden] public Component? FindComponent(string[] names) => Root.FindComponent(names);
    
    public Obj? Find(Table t) => Root.Find(t.Values.Select(v => v.String).ToArray());
    public Component? FindComponent(Table t) => Root.FindComponent(t.Values.Select(v => v.String).ToArray());
}