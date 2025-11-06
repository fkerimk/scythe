using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public unsafe class animation(obj obj) : type(obj) {
    
    public ModelAnimation* rl_anims;
    
    public int count;
    public string path;

    public bool anim_loaded;
    
    public int anim = 0;
    public int frame = 0;
    public float frame_raw = 0f;

    public override void loop() {
        
        if (!anim_loaded) {

            rl_anims = Raylib.LoadModelAnimations(path, ref count);
            anim_loaded = true;
        }

        if (obj.parent.type_class is model model) {

            frame = (int)Math.Floor(frame_raw);
            if (frame >= rl_anims[anim].FrameCount) frame_raw = 0;
            
            Raylib.UpdateModelAnimation(model.rl_model, rl_anims[anim], frame);
            
            frame_raw += Raylib.GetFrameTime() * 60;
        }
    }
}