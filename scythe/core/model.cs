using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class model(obj obj) : type(obj) {

    public Model rl_model;
    
    public string path;

    public bool model_loaded;

    public override void loop() {

        if (!model_loaded) {

            rl_model = Raylib.LoadModel(path);
            model_loaded = true;
        }

        rl_model.Transform = obj.parent!.matrix;
        
        Raylib.DrawModel(rl_model, Vector3.Zero, 1, Color.White);
        
        //Raylib.DrawModelEx(
        //    rl_model,
        //    new(obj.parent!.prs.px, obj.parent!.prs.py, obj.parent!.prs.pz),
        //    new(obj.parent!.prs.rx, obj.parent!.prs.ry, obj.parent!.prs.rz),
        //    35,
        //    new(obj.parent!.prs.sx, obj.parent!.prs.sy, obj.parent!.prs.sz),
        //    new(1f, 1f, 1f, 1f)
        //);
    }
}