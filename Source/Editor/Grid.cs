using System.Numerics;
using Raylib_cs;

internal class Grid(Cam cam) {

    public void Draw() {
        
        var gridPos = new float3(
            (int)cam.Pos.x, 0,
            (int)cam.Pos.z
        );
            
        Rlgl.PushMatrix();
        Rlgl.Translatef(gridPos.x, gridPos.y, gridPos.z);

        const int slices = 100;
        const float spacing = 1f;
        
        const float half = slices * spacing;

        for (var i = 0; i <= slices * 2; i++) {
                
            var pos = -half + i * spacing;
                
            var startX = new Vector3(-half, 0, pos);
            var endX = new Vector3(half, 0, pos);
            Raylib.DrawLine3D(startX, endX, Colors.Grid.to_raylib());

            var startZ = new Vector3(pos, 0, -half);
            var endZ = new Vector3(pos, 0, half);
            Raylib.DrawLine3D(startZ, endZ, Colors.Grid.to_raylib());
        }
        
        Rlgl.PopMatrix();
    }
}