using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Model(Obj obj) : ObjType(obj) {

    public override int Priority => 2;
    
    public override string LabelIcon => Icons.Model;
    public override ScytheColor LabelScytheColor => Colors.GuiTypeModel;
    
    [RecordHistory] [JsonProperty] [Label("Path")] public string Path { get; set; } = "";
    [RecordHistory] [JsonProperty] [Label("Color")] public ScytheColor ScytheColor { get; set; } = Colors.White;

    public Raylib_cs.Model RlModel;

    public override unsafe bool Load() {
        
        if (!PathUtil.BestPath($"Models/{Path}.iqm", out var modelPath)) return false;
        
        RlModel = Raylib.LoadModel(modelPath);
                
        if (CommandLine.Editor ? Config.Editor.GenTangents : Config.Runtime.GenTangents)
            for (var m = 0; m < RlModel.MeshCount; m++)
                Raylib.GenMeshTangents(ref RlModel.Meshes[m]);

        if (CommandLine.Editor ? Config.Editor.NoShade : Config.Runtime.NoShade) return true;
        
        for (var i = 0; i < RlModel.MaterialCount; i++) {
                    
            RlModel.Materials[i].Shader = Shaders.Pbr;
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value = 0f;
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value = 0.5f;
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Occlusion].Value = 1;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value = 0;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value = 0.5f;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Occlusion].Value = 1f;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Emission].Color = Raylib_cs.Color.Black;

            // Default flat normal map (128, 128, 255) = (0, 0, 1) in normalized space
            var normalImg = Raylib.GenImageColor(1, 1, new Raylib_cs.Color(128, 128, 255, 255));
            var normalTex = Raylib.LoadTextureFromImage(normalImg);
            Raylib.UnloadImage(normalImg);
        
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Normal].Texture = normalTex;
        
            //var mraImg = Raylib.GenImageColor(1, 1, Color.White);
            //var mraTex = Raylib.LoadTextureFromImage(mraImg);
            //Raylib.UnloadImage(mraImg);
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Texture = mraTex;
        }

        return true;
    }

    public override unsafe void Loop3D() {

        RlModel.Transform = Obj.Parent!.WorldMatrix;
        
        // Draw test model
        for (var i = 0; i < RlModel.MaterialCount; i++) {
            var emissiveColor = Raylib.ColorNormalize(RlModel.Materials[i].Maps[(int)MaterialMapIndex.Emission].Color);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrEmissiveColor, emissiveColor, ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrMetallicValue, RlModel.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrRoughnessValue, RlModel.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value, ShaderUniformDataType.Float);
        }

        Raylib.SetShaderValue(Shaders.Pbr, Shaders.PbrTiling, new Vector2(0.5f, 0.5f), ShaderUniformDataType.Vec2);
        Raylib.DrawModel(RlModel, Vector3.Zero, 1, ScytheColor.ToRaylib());
    }

    public override void Quit() {
        
        Raylib.UnloadModel(RlModel);
    }
}