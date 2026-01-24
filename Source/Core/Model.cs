using Raylib_cs;
using Newtonsoft.Json;
using static Raylib_cs.Raylib;

internal class Model(Obj obj) : Component(obj) {

    public override string LabelIcon => Icons.FaCube;
    public override Color LabelColor => Colors.GuiTypeModel;
    
    [RecordHistory] [JsonProperty] [Label("Path")] [FindAsset("ModelAsset")] public string Path { get; set; } = "";
    [RecordHistory] [JsonProperty] [Label("Color")] public Color Color { get; set; } = Color.White;
    [RecordHistory] [JsonProperty] [Label("Transparent")] public bool IsTransparent { get; set; }
    [RecordHistory] [JsonProperty] [Label("Alpha Cutoff")] public float AlphaCutoff { get; set; } = 0.5f;
    [RecordHistory] [JsonProperty] [Label("Cast Shadows")] public bool CastShadows { get; set; } = true;
    [RecordHistory] [JsonProperty] [Label("Receive Shadows")] public bool ReceiveShadows { get; set; } = true;
    
    public List<AssimpMesh> Meshes = [];
    public List<BoneInfo> Bones = [];
    public ModelAsset AssetRef = null!;

    public override bool Load() {
        
        var loaded = AssetManager.Get<ModelAsset>(Path);
        if (loaded is not { IsLoaded: true }) return false;
        
        // Ensure lists are clear before adding
        Meshes.Clear();
        Bones.Clear();
        
        AssetRef = loaded;
        foreach (var m in AssetRef.Meshes) Meshes.Add(m.Clone());
        foreach (var b in AssetRef.Bones) Bones.Add(new BoneInfo { Name = b.Name, Index = b.Index, Offset = b.Offset });
        return true;
    }

    public override void Logic() { }
    public override void Render3D() { if (!IsTransparent) Draw(); }

    public void DrawTransparent() {
        
        BeginBlendMode(BlendMode.Alpha);
        Draw();
        EndBlendMode();
    }

    public void DrawShadow() { if (CastShadows) Draw(); }

    public void Draw(float? overrideAlphaCutoff = null) {
        
        if (AssetRef is not { IsLoaded: true }) return;
        
        // 1. Optimize: Global material update check (only if anything changed)
        AssetRef.UpdateMaterialsIfDirty();

        MaterialAsset? lastMatAsset = null;
        uint lastShaderId = 0;
        uint lastMatVersion = 0;

        foreach (var mesh in Meshes) {
            
            var material = (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < AssetRef.Materials.Length) 
                ? AssetRef.Materials[mesh.MaterialIndex] 
                : MaterialAsset.Default.Material;

            var matAsset = (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < AssetRef.CachedMaterialAssets.Count)
                ? (AssetRef.CachedMaterialAssets[mesh.MaterialIndex] ?? MaterialAsset.Default)
                : MaterialAsset.Default;

            // 2. Resolve Material Asset parameters (only for shared shader values)
            var shader = material.Shader;

            if (matAsset != lastMatAsset || shader.Id != lastShaderId || matAsset.Version != lastMatVersion) {
                
                matAsset.ApplyUniforms(shader);
                lastMatAsset = matAsset;
                lastShaderId = shader.Id;
                lastMatVersion = matAsset.Version;
            }

            // 3. Batch apply uniforms (Only if they exist in shader)
            var loc = GetShaderLocation(shader, "albedo_color");
            if (loc != -1) SetShaderValue(shader, loc, ColorNormalize(Color), ShaderUniformDataType.Vec4);

            loc = GetShaderLocation(shader, "receive_shadows");
            if (loc != -1) SetShaderValue(shader, loc, ReceiveShadows ? 1 : 0, ShaderUniformDataType.Int);

            loc = GetShaderLocation(shader, "alpha_cutoff");
            if (loc != -1) SetShaderValue(shader, loc, overrideAlphaCutoff ?? AlphaCutoff, ShaderUniformDataType.Float);

            // Global Ambient (Live Update)
            var locAmbInt = GetShaderLocation(shader, "ambient_intensity");
            if (locAmbInt != -1) SetShaderValue(shader, locAmbInt, Core.RenderSettings.AmbientIntensity, ShaderUniformDataType.Float);

            var locAmbCol = GetShaderLocation(shader, "ambient_color");
            if (locAmbCol != -1) SetShaderValue(shader, locAmbCol,Core.RenderSettings.AmbientColor.ToVector4(), ShaderUniformDataType.Vec3);
            
            // 4. Draw
            var matModel = Obj.VisualWorldMatrix;
            if (AssetRef.Settings.ImportScale != 1.0f) {
                
                var s = AssetRef.Settings.ImportScale;
                
                // Scale only the basis vectors (rotation/scale) and NOT the translation (M41, M42, M43)
                matModel.M11 *= s; matModel.M12 *= s; matModel.M13 *= s;
                matModel.M21 *= s; matModel.M22 *= s; matModel.M23 *= s;
                matModel.M31 *= s; matModel.M32 *= s; matModel.M33 *= s;
            }
            
            DrawMesh(mesh.RlMesh, material, matModel);
        }
    }

    public override void Unload() { 
        foreach (var m in Meshes) UnloadMesh(m.RlMesh);
        Meshes.Clear();
        Bones.Clear();
    }
}
