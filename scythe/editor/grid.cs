using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public class grid(cam cam) {

    public void draw() {
        
        var grid_pos = new float3(
            (int)cam.pos.x, 0,
            (int)cam.pos.z
        );
            
        Rlgl.PushMatrix();
        Rlgl.Translatef(grid_pos.x, grid_pos.y, grid_pos.z);

        const int slices = 100;
        const float spacing = 1f;
        
        const float half = slices * spacing;

        for (var i = 0; i <= slices * 2; i++) {
                
            var pos = -half + i * spacing;
                
            var start_x = new Vector3(-half, 0, pos);
            var end_x = new Vector3(half, 0, pos);
            Raylib.DrawLine3D(start_x, end_x, colors.grid.to_raylib());

            var start_z = new Vector3(pos, 0, -half);
            var end_z = new Vector3(pos, 0, half);
            Raylib.DrawLine3D(start_z, end_z, colors.grid.to_raylib());
        }
        
        Rlgl.PopMatrix();
    }
}