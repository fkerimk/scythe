using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class transform(obj obj) : type(obj) {
    
    [label("Pos")] public float3 pos { get => _pos; set => _pos = value; }
    [label("Rot")] public float3 rot { get => _rot; set => _rot = value; }
    [label("Scale")] public float3 scale { get => _scale; set => _scale = value; }
    
    private float3 _pos = float3.zero;
    private float3 _rot  = float3.zero;
    private float3 _scale  = float3.one;

    public override void loop_3d(bool is_editor) {

        
        //pos.y = MathF.Sin((float)Raylib.GetTime() * 5) * 0.1f;
        //scale.y = 1 + MathF.Sin((float)Raylib.GetTime() * 10) * 0.2f;
        //scale.x = 1 - MathF.Sin((float)Raylib.GetTime() * 10) * 0.2f;
        //scale.z = 1 - MathF.Sin((float)Raylib.GetTime() * 10) * 0.2f;
        //rot.y += Raylib.GetFrameTime() * 180;
        
        float to_rad(float deg) => deg * MathF.PI / 180f;
        
        obj.parent!.matrix = Raymath.MatrixMultiply(
            
            Raymath.MatrixMultiply(
                
                Raymath.MatrixScale(scale.x, scale.y, scale.z),
                Raymath.MatrixRotateXYZ(new(to_rad(rot.x), to_rad(rot.y), to_rad(rot.z)))
            ),
            
            Raymath.MatrixTranslate(pos.x, pos.y, pos.z)
        );
    }

    public override void loop_ui(bool is_editor) {}

    public override void loop_editor(viewport viewport) {

        var mouse_pos = viewport.relative_mouse;
    }

    public override void quit() {}
}