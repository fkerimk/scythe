using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using Newtonsoft.Json;

internal class ModelAsset : Asset {
    
    public List<AssimpMesh> Meshes = [];
    public List<BoneInfo> Bones = [];
    public ModelNode RootNode = null!;
    public Matrix4x4 GlobalInverse;
    public List<AnimationClip> Animations = [];
    public Material[] Materials = null!;
    public string[] MaterialPaths = null!;
    public List<MaterialAsset?> CachedMaterialAssets = [];
    
    private uint _lastBakeVersion;

    public class ModelSettings {
        public readonly Dictionary<int, string> MeshMaterials = new();
    }
    public ModelSettings Settings = new();

    public override bool Load() {
        
        if (!System.IO.File.Exists(File)) return false;
        
        try {
            
            var data = AssimpLoader.Load(File);
            Meshes = data.Meshes; Bones = data.Bones; RootNode = data.Root; GlobalInverse = data.GlobalInverse; Animations = data.Animations;
            var jsonPath = File + ".json";
            if (System.IO.File.Exists(jsonPath)) Settings = JsonConvert.DeserializeObject<ModelSettings>(System.IO.File.ReadAllText(jsonPath)) ?? new ModelSettings();
            
        } catch { return false; }

        IsLoaded = true;
        var matCount = Meshes.Count > 0 ? Meshes.Max(m => m.MaterialIndex) + 1 : 1;
        Materials = new Material[matCount];
        MaterialPaths = new string[matCount];
        CachedMaterialAssets = [];

        for (var i = 0; i < matCount; i++) {
            
            Materials[i] = LoadMaterialDefault();
            MaterialPaths[i] = Settings.MeshMaterials.GetValueOrDefault(i, "");
            CachedMaterialAssets.Add(null);
            ApplyMaterialState(i, true);
        }
        
        return true;
    }

    public void SaveSettings() {
        
        var jsonPath = File + ".json";
        for (var i = 0; i < MaterialPaths.Length; i++) Settings.MeshMaterials[i] = MaterialPaths[i];
        System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(Settings, Formatting.Indented));
    }

    public void ApplyMaterial(int index, string path) {
        
        if (index < 0 || index >= Materials.Length) return;
        MaterialPaths[index] = path;
        CachedMaterialAssets[index] = AssetManager.Get<MaterialAsset>(path);
        ApplyMaterialState(index, true);
        SaveSettings();
    }

    public void UpdateMaterialsIfDirty() {
        
        if (_lastBakeVersion == MaterialAsset.GlobalVersion) return;
        for (var i = 0; i < Materials.Length; i++) ApplyMaterialState(i);
        _lastBakeVersion = MaterialAsset.GlobalVersion;
    }

    public unsafe void ApplyMaterialState(int index, bool force = false) {
        
        if (index < 0 || index >= Materials.Length) return;
        
        ref var mat = ref Materials[index];
        var asset = (index < CachedMaterialAssets.Count) ? CachedMaterialAssets[index] : null;

        if (asset == null && !string.IsNullOrEmpty(MaterialPaths[index])) {
            
            asset = AssetManager.Get<MaterialAsset>(MaterialPaths[index]);
            if (index < CachedMaterialAssets.Count) CachedMaterialAssets[index] = asset;
        }
        
        var shaderAsset = AssetManager.Get<ShaderAsset>(asset?.Data.Shader ?? "pbr") ?? AssetManager.Get<ShaderAsset>("pbr");
        if (shaderAsset == null) return;
        
        mat.Shader = shaderAsset.Shader;

        // Texture Assignment (Baked into Material Struct)
        fixed (Material* p = &mat) {
            
            var tPath = asset?.Data.Textures.GetValueOrDefault("albedo_map", "");
            var tex = AssetManager.Get<TextureAsset>(tPath);
            SetMaterialTexture(p, MaterialMapIndex.Albedo, tex?.Texture ?? new Texture2D());

            tPath = asset?.Data.Textures.GetValueOrDefault("normal_map", "");
            tex = AssetManager.Get<TextureAsset>(tPath);
            SetMaterialTexture(p, MaterialMapIndex.Normal, tex?.Texture ?? new Texture2D());

            tPath = asset?.Data.Textures.GetValueOrDefault("metallic_map", "");
            tex = AssetManager.Get<TextureAsset>(tPath);
            SetMaterialTexture(p, MaterialMapIndex.Metalness, tex?.Texture ?? new Texture2D());

            tPath = asset?.Data.Textures.GetValueOrDefault("roughness_map", "");
            tex = AssetManager.Get<TextureAsset>(tPath);
            SetMaterialTexture(p, MaterialMapIndex.Roughness, tex?.Texture ?? new Texture2D());

            tPath = asset?.Data.Textures.GetValueOrDefault("occlusion_map", "");
            tex = AssetManager.Get<TextureAsset>(tPath);
            SetMaterialTexture(p, MaterialMapIndex.Occlusion, tex?.Texture ?? new Texture2D());

            tPath = asset?.Data.Textures.GetValueOrDefault("emissive_map", "");
            tex = AssetManager.Get<TextureAsset>(tPath);
            SetMaterialTexture(p, MaterialMapIndex.Emission, tex?.Texture ?? new Texture2D());
        }
    }
    
    public override void Unload() {
        
        foreach (var mesh in Meshes) UnloadMesh(mesh.RlMesh);
        foreach (var mat in Materials) UnloadMaterial(mat);
    }
}
