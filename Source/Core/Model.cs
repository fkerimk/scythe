using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;

internal class Model(Obj obj) : Component(obj) {

    public override string LabelIcon => Icons.FaCube;
    public override Color LabelColor => Colors.GuiTypeModel;
    
    [RecordHistory] [JsonProperty] [Label("Path")] [FilePath("Models", ".iqm")] public string Path { get; set; } = "";
    [RecordHistory] [JsonProperty] [Label("Color")] public Color Color { get; set; } = Color.White;
    [RecordHistory] [JsonProperty] [Label("Transparent")] public bool IsTransparent { get; set; }
    [RecordHistory] [JsonProperty] [Label("Alpha Cutoff")] public float AlphaCutoff { get; set; } = 0.5f;
    [RecordHistory] [JsonProperty] [Label("Cast Shadows")] public bool CastShadows { get; set; } = true;
    [RecordHistory] [JsonProperty] [Label("Receive Shadows")] public bool ReceiveShadows { get; set; } = true;
    
    public ModelAsset Asset = null!;
    public Raylib_cs.Model RlModel;

    public override bool Load() {
        
        if (!PathUtil.BestPath($"Models/{Path}.iqm", out var modelPath)) return false;
        if (!AssetManager.Load(modelPath, out Asset!)) return false;

        // Create a local copy of the model structure (shallow copy)
        // This ensures this component has its own transform and bone state
        RlModel = Asset.RlModel; 
        
        return true;
    }

    public override void Logic() => RlModel.Transform = Obj.WorldMatrix;

    public override void Render3D() {
        
        if (IsTransparent) return;
        Draw();
    }

    public void DrawTransparent() {
        
        Raylib.BeginBlendMode(BlendMode.Alpha);
        Draw();
        Raylib.EndBlendMode();
    }

    public void DrawShadow() {
        
        if (!CastShadows) return;
        Draw();
    }

    public unsafe void Draw() {
        
        for (var i = 0; i < RlModel.MaterialCount; i++) {
            
            var emissiveColor = Raylib.ColorNormalize(RlModel.Materials[i].Maps[(int)MaterialMapIndex.Emission].Color);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrEmissiveColor, emissiveColor, ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrMetallicValue, RlModel.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrRoughnessValue, RlModel.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value, ShaderUniformDataType.Float);
        }

        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrTiling, new Vector2(0.5f, 0.5f), ShaderUniformDataType.Vec2);
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrAlphaCutoff, AlphaCutoff, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrReceiveShadows, ReceiveShadows ? 1 : 0, ShaderUniformDataType.Int);
        
        Raylib.DrawModel(RlModel, Vector3.Zero, 1, Color);
    }
}