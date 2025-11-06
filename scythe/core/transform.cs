using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class transform(obj obj) : type(obj) {

    public float3 pos;
    public float3 rot;
    public float3 scale = float3.one;

    public override void loop() {
        
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
}