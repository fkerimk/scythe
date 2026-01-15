using System.Numerics;
using Raylib_cs;

internal class Grid(Camera3D? camera) {

    public void Draw() {
        
        if (camera == null) return;
        
        var gridPos = new Vector3((int)camera.Position.X, 0, (int)camera.Position.Z);
            
        Rlgl.PushMatrix();
        Rlgl.Translatef(gridPos.X, gridPos.Y, gridPos.Z);

        const int slices = 100;
        const float spacing = 1f;
        const float half = slices * spacing;

        for (var i = 0; i <= slices * 2; i++) {
                
            var pos = -half + i * spacing;
                
            var startX = new Vector3(-half, 0, pos);
            var endX = new Vector3(half, 0, pos);
            Raylib.DrawLine3D(startX, endX, Colors.Grid.ToRaylib());

            var startZ = new Vector3(pos, 0, -half);
            var endZ = new Vector3(pos, 0, half);
            Raylib.DrawLine3D(startZ, endZ, Colors.Grid.ToRaylib());
        }
        
        Rlgl.PopMatrix();
    }
}