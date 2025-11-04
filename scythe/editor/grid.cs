using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public static class grid {

    public static void draw() {
        
        var gridpos = new Vector3(
            (int)cam.current.Position.X, 0,
            (int)cam.current.Position.Z
        );
            
        Rlgl.PushMatrix();
        Rlgl.Translatef(gridpos.X, gridpos.Y, gridpos.Z);

        const int slices = 100;
        const float spacing = 1f;
        
        const float half = slices * spacing;

        for (var i = 0; i <= slices * 2; i++) {
                
            var pos = -half + i * spacing;
                
            var start_x = new Vector3(-half, 0, pos);
            var end_x = new Vector3(half, 0, pos);
            Raylib.DrawLine3D(start_x, end_x, colors.grid);

            var start_z = new Vector3(pos, 0, -half);
            var end_z = new Vector3(pos, 0, half);
            Raylib.DrawLine3D(start_z, end_z, colors.grid);
        }
        
        Rlgl.PopMatrix();
    }
}