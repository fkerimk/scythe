using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;
using static Raylib_cs.Raylib;

internal class MaterialAsset : Asset {
    
    public static uint GlobalVersion { get; private set; } = 1;

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

        var shaderAsset = AssetManager.Get<ShaderAsset>(Data.Shader);
        if (shaderAsset != null) Material.Shader = shaderAsset.Shader;

        foreach (var texKv in Data.Textures) {
            
            if (!Enum.TryParse<MaterialMapIndex>(texKv.Key, true, out var index)) continue;
            
            var texAsset = AssetManager.Get<TextureAsset>(texKv.Value);
            
            if (texAsset is { IsLoaded: true })
                fixed (Material* matPtr = &Material) SetMaterialTexture(matPtr, index, texAsset.Texture);
        }

        if (shaderAsset == null) return;
        
        foreach (var kv in Data.Colors) {
            
            var loc = shaderAsset.GetLoc(kv.Key);
            if (loc != -1) SetShaderValue(shaderAsset.Shader, loc, ColorNormalize(kv.Value), ShaderUniformDataType.Vec4);
        }
        
        foreach (var kv in Data.Vectors) {
            
            var loc = shaderAsset.GetLoc(kv.Key);
            if (loc != -1) SetShaderValue(shaderAsset.Shader, loc, kv.Value, ShaderUniformDataType.Vec2);
        }
        
        foreach (var kv in Data.Floats) {
            
            var loc = shaderAsset.GetLoc(kv.Key);
            if (loc != -1) SetShaderValue(shaderAsset.Shader, loc, kv.Value, ShaderUniformDataType.Float);
        }
        
        foreach (var kv in Data.Ints) {
            
            var loc = shaderAsset.GetLoc(kv.Key);
            if (loc != -1) SetShaderValue(shaderAsset.Shader, loc, kv.Value, ShaderUniformDataType.Int);
        }
    }

    public override void Unload() { IsLoaded = false; }
    
    public void Save() {
        
        var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
        System.IO.File.WriteAllText(File, json);
    }
}
