using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class model(obj obj) : type(obj) {

    [label("Path")] public string path { get; set; } = "";
    [label("Color")] public color color { get; set; } = new(1,1,1,1);

    public Model rl_model;
    private bool model_loaded;
    
    public override void loop_3d(bool is_editor) {

        if (!model_loaded) {

            rl_model = Raylib.LoadModel(scythe.path.relative(path));
            model_loaded = true;
        }

        rl_model.Transform = obj.parent!.matrix;
        
        Raylib.DrawModel(rl_model, Vector3.Zero, 1, color.to_raylib());
    }

    public override void loop_ui(bool is_editor) {}
    public override void loop_editor(viewport viewport) { }

    public override void quit() {
        
        Raylib.UnloadModel(rl_model);
    }
}