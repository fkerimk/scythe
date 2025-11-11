using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public unsafe class animation(obj obj) : type(obj) {
    
    public override string label_icon => icons.animation;
    public override color label_color => colors.gui_type_animation;

    [label("Path")] public string path { get; set; } = "";

    [label("Track")] public int track { get; set {

        if (count == -1) field = value;
        else if (count == 0 || value < 0) field = 0;
        else if (value >= count) field = count - 1;
        else field = value;
    } }

    [label("Is Playing")] public bool is_playing { get; set {

        if (!field && value) {

            frame = 0;
            frame_raw = 0;
        }

        field = value;
        
    } } = true;

    [label("Looping")] public bool looping { get; set; } = true;

    private int count = -1;
    private int frame;
    private float frame_raw;
    
    private bool anim_loaded;
    private ModelAnimation* rl_anims;
    
    public override void loop_3d(bool is_editor) {
        
        if (!anim_loaded) {

            rl_anims = Raylib.LoadModelAnimations(scythe.path.relative(path), ref count);
            anim_loaded = true;
        }

        else if (is_playing && obj.parent?.type is model model) {
                
            frame = (int)Math.Floor(frame_raw);
            
            if (frame >= rl_anims[track].FrameCount) {

                frame = 0;
                frame_raw = 0;

                is_playing = looping;
            }
        
            Raylib.UpdateModelAnimation(model.rl_model, rl_anims[track], frame);
            
            frame_raw += Raylib.GetFrameTime() * 60;
        }
    }

    public override void loop_ui(bool is_editor) {}
    public override void loop_3d_editor(viewport viewport) { }
    public override void loop_ui_editor(viewport viewport) { }

    public override void quit() {
        
        Raylib.UnloadModelAnimations(rl_anims, count);
    }
}