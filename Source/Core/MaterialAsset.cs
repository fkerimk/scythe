using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using static Raylib_cs.Raylib;

internal class MaterialAsset : Asset {
    
    public static uint GlobalVersion { get; private set; } = 1;
    public uint Version { get; private set; } = 1;

    public Material Material;
    public MaterialData Data = new();
    
    public class MaterialData {
        
        public string Shader = "Pbr";
        public readonly Dictionary<string, string> Textures = new();
        public readonly Dictionary<string, float> Floats = new();
        public readonly Dictionary<string, int> Ints = new();
        public readonly Dictionary<string, Color> Colors = new();
        public readonly Dictionary<string, Vector2> Vectors = new();
    }

    public static MaterialAsset Default {
        
        get {
            
            field ??= new MaterialAsset {
                
                IsLoaded = true,
                Data = new MaterialData(),
                Material = LoadMaterialDefault(),
                File = "Default"
            };
            
            return field;
        }
    }

    public override bool Load() {
        
        if (!System.IO.File.Exists(File)) return false;
        
        try {
            var json = System.IO.File.ReadAllText(File);
            Data = JsonConvert.DeserializeObject<MaterialData>(json) ?? new MaterialData();
        } catch { return false; }

        Material = LoadMaterialDefault();
        IsLoaded = true;
        ApplyChanges();
        return true;
    }
    
    public unsafe void ApplyChanges() {
        
        if (!IsLoaded) return;
        
        GlobalVersion++;
        Version++;

        var shaderAsset = AssetManager.Get<ShaderAsset>(Data.Shader);
        if (shaderAsset != null) Material.Shader = shaderAsset.Shader;

        foreach (var texKv in Data.Textures) {
            
            if (!Enum.TryParse<MaterialMapIndex>(texKv.Key, true, out var index)) continue;
            
            var texAsset = AssetManager.Get<TextureAsset>(texKv.Value);
            
            if (texAsset is { IsLoaded: true })
                fixed (Material* matPtr = &Material) SetMaterialTexture(matPtr, index, texAsset.Texture);
        }
    }

    public void ApplyUniforms(Shader shader) {
        
        var shaderAsset = AssetManager.Get<ShaderAsset>(Data.Shader);
        if (shaderAsset == null) return;

        foreach (var prop in shaderAsset.Properties) {
            
            var loc = shaderAsset.GetLoc(prop.Name);
            if (loc == -1) continue;

                switch (prop.Type) {
                    
                case "float": {
                    
                    var val = Data.Floats.GetValueOrDefault(prop.Name, prop.Name.Contains("roughness") ? 0.5f : 0f);
                    SetShaderValue(shader, loc, val, ShaderUniformDataType.Float);
                    break;
                }
                
                case "int": {
                    
                    var val = Data.Ints.GetValueOrDefault(prop.Name, 0);
                    SetShaderValue(shader, loc, val, ShaderUniformDataType.Int);
                    break;
                }
                
                case "vec2": {
                    
                    var val = Data.Vectors.GetValueOrDefault(prop.Name, prop.Name == "tiling" ? Vector2.One : Vector2.Zero);
                    SetShaderValue(shader, loc, val, ShaderUniformDataType.Vec2);
                    break;
                }
                
                case "vec3": {
                    
                    // Note: We don't have a Vec3 dictionary, but we can use Colors or just default to zero
                    var val = Vector3.Zero;
                    if (Data.Colors.TryGetValue(prop.Name, out var col)) val = new Vector3(col.R / 255f, col.G / 255f, col.B / 255f);
                    SetShaderValue(shader, loc, val, ShaderUniformDataType.Vec3);
                    break;
                }
                
                case "vec4": {
                    
                    var val = Data.Colors.TryGetValue(prop.Name, out var col) ? ColorNormalize(col) : Vector4.One;
                    SetShaderValue(shader, loc, val, ShaderUniformDataType.Vec4);
                    break;
                }
            }
        }
    }

    public override void Unload() { IsLoaded = false; }
    
    public void Save() {
        
        var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
        System.IO.File.WriteAllText(File, json);
    }
}
