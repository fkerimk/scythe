using Raylib_cs;
using static Raylib_cs.Raylib;

internal class ModelAsset : Asset {
    
    public Raylib_cs.Model RlModel;
    
    public override unsafe bool Load() {
        
        if (Path.GetExtension(File) != ".iqm" || !System.IO.File.Exists(File)) return false;
        
        RlModel = LoadModel(File);
        IsLoaded = true;
                
        if (CommandLine.Editor ? Config.Editor.GenTangents : Config.Runtime.GenTangents)
            for (var m = 0; m < RlModel.MeshCount; m++)
                GenMeshTangents(ref RlModel.Meshes[m]);

        if (CommandLine.Editor ? Config.Editor.NoShade : Config.Runtime.NoShade) return true;
        
        for (var i = 0; i < RlModel.MaterialCount; i++) {
                    
            RlModel.Materials[i].Shader = Shaders.Pbr;
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value = 0f;
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value = 0.5f;
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Occlusion].Value = 1;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value = 0;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value = 0.5f;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Occlusion].Value = 1f;
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Emission].Color = Color.Black;

            // Default flat normal map (128, 128, 255) = (0, 0, 1) in normalized space
            var normalImg = GenImageColor(1, 1, new Color(128, 128, 255, 255));
            var normalTex = LoadTextureFromImage(normalImg);
            UnloadImage(normalImg);
        
            RlModel.Materials[i].Maps[(int)MaterialMapIndex.Normal].Texture = normalTex;
        
            //var mraImg = Raylib.GenImageColor(1, 1, Color.White);
            //var mraTex = Raylib.LoadTextureFromImage(mraImg);
            //Raylib.UnloadImage(mraImg);
            //rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Texture = mraTex;
        }

        return true;
    }
    
    public override void Unload() => UnloadModel(RlModel);
}