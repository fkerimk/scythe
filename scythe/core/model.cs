using System.Numerics;
using Raylib_cs;

namespace scythe;

public class model(obj obj) : type(obj) {

    public override string label_icon => icons.model;
    public override color label_color => colors.gui_type_model;
    
    [label("Path")] public string path { get; set; } = "";
    [label("Color")] public color color { get; set; } = colors.white;

    public Model rl_model;
    private bool model_loaded;
    
    public override unsafe void loop_3d(bool is_editor) {

        if (!model_loaded) {

            rl_model = Raylib.LoadModel(scythe.path.relative(path));
            
            if (is_editor ? config.editor.gen_tangents : config.runtime.gen_tangents)
                for (var m = 0; m < rl_model.MeshCount; m++)
                    Raylib.GenMeshTangents(ref rl_model.Meshes[m]);
    
            if (is_editor ? !config.editor.no_shade : !config.runtime.no_shade) for (var i = 0; i < rl_model.MaterialCount; i++) {
                
                rl_model.Materials[i].Shader = shaders.pbr;
                rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value = 0f;
                rl_model.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value = 0.5f;
                rl_model.Materials[i].Maps[(int)MaterialMapIndex.Occlusion].Value = 1;
                rl_model.Materials[i].Maps[(int)MaterialMapIndex.Emission].Color = Color.Black;

                // Default flat normal map (128, 128, 255) = (0, 0, 1) in normalized space
                var normalImg = Raylib.GenImageColor(1, 1, new Color(128, 128, 255, 255));
                var normalTex = Raylib.LoadTextureFromImage(normalImg);
                Raylib.UnloadImage(normalImg);
    
                rl_model.Materials[i].Maps[(int)MaterialMapIndex.Normal].Texture = normalTex;
    
                var mraImg = Raylib.GenImageColor(1, 1, Color.White);
                var mraTex = Raylib.LoadTextureFromImage(mraImg);
                Raylib.UnloadImage(mraImg);
                rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Texture = mraTex;
            }
            
            model_loaded = true;
        }

        rl_model.Transform = obj.parent!.matrix;
        
        // Draw test model - tüm materyalleri set et
        for (var i = 0; i < rl_model.MaterialCount; i++) {
            var emissive_color = Raylib.ColorNormalize(rl_model.Materials[i].Maps[(int)MaterialMapIndex.Emission].Color);
            Raylib.SetShaderValue(shaders.pbr, shaders.emissiveColorLoc, emissive_color, ShaderUniformDataType.Vec4);
            Raylib.SetShaderValue(shaders.pbr, shaders.metallicValueLoc, rl_model.Materials[i].Maps[(int)MaterialMapIndex.Metalness].Value, ShaderUniformDataType.Float);
            Raylib.SetShaderValue(shaders.pbr, shaders.roughnessValueLoc, rl_model.Materials[i].Maps[(int)MaterialMapIndex.Roughness].Value, ShaderUniformDataType.Float);
        }

        Raylib.SetShaderValue(shaders.pbr, shaders.textureTilingLoc, new Vector2(0.5f, 0.5f), ShaderUniformDataType.Vec2);
        Raylib.DrawModel(rl_model, Vector3.Zero, 1, color.to_raylib());
    }

    public override void loop_ui(bool is_editor) {}
    public override void loop_3d_editor(viewport viewport) { }
    public override void loop_ui_editor(viewport viewport) { }

    public override void quit() {
        
        Raylib.UnloadModel(rl_model);
    }
}