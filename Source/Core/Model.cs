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

    public void Draw() {
        
        if (AssetRef is not { IsLoaded: true }) return;
        
        // 1. Optimize: Global material update check (only if anything changed)
        AssetRef.UpdateMaterialsIfDirty();

        foreach (var mesh in Meshes) {
            var material = (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < AssetRef.Materials.Length) 
                ? AssetRef.Materials[mesh.MaterialIndex] 
                : MaterialAsset.Default.Material;

            // 2. Resolve Material Asset parameters (only for shared shader values)
            var shader = material.Shader;

            // 3. Batch apply uniforms (Only if they exist in shader)
            var loc = GetShaderLocation(shader, "albedo_color");
            if (loc != -1) SetShaderValue(shader, loc, ColorNormalize(Color), ShaderUniformDataType.Vec4);

            loc = GetShaderLocation(shader, "receive_shadows");
            if (loc != -1) SetShaderValue(shader, loc, ReceiveShadows ? 1 : 0, ShaderUniformDataType.Int);

            loc = GetShaderLocation(shader, "alpha_cutoff");
            if (loc != -1) SetShaderValue(shader, loc, AlphaCutoff, ShaderUniformDataType.Float);

            // 4. Draw
            DrawMesh(mesh.RlMesh, material, Obj.VisualWorldMatrix);
        }
    }

    public override void Unload() { foreach (var m in Meshes) UnloadMesh(m.RlMesh); }
}
