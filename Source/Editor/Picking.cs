using System.Numerics;
using Raylib_cs;

internal static class Picking {

    public static unsafe void Update() {

        if (Core.ActiveCamera == null || Core.ActiveLevel == null) return;
        
        // Ensure viewport interaction is valid and prevent picking when clicking UI
        if (!Editor.Level3D.IsHovered) return;
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;

        // Perform raycasting using the camera and the normalized mouse position 
        // specifically mapped for the 3D viewport dimensions in Level3D.cs
        // Fixed: Use GetScreenToWorldRay instead of obsolete GetMouseRay
        var ray = Raylib.GetScreenToWorldRay(Level3D.RelativeMouse3D, Core.ActiveCamera.Raylib);
        
        Obj? closestObj = null;
        var minDistance = float.MaxValue;

        // Start recursive search from the root of the active level
        CheckObj(Core.ActiveLevel.Root);

        void CheckObj(Obj obj) {

            foreach (var component in obj.Components.Values) {
                
                // Only perform collision check if the model is loaded
                if (component is Model { IsLoaded: true } model) {
                    
                    for (var i = 0; i < model.RlModel.MeshCount; i++) {
                        
                        // %100 accurate collision check against the actual mesh triangles
                        // model.RlModel.Transform is the World Matrix of the object
                        // Fixed: Added unsafe context for Raylib.GetRayCollisionMesh
                        var collision = Raylib.GetRayCollisionMesh(ray, model.RlModel.Meshes[i], model.RlModel.Transform);
                        
                        if (collision.Hit && collision.Distance < minDistance) {
                            minDistance = collision.Distance;
                            closestObj = obj;
                        }
                    }
                }
            }

            // Traverse children recursively
            foreach (var child in obj.Children.Values)
                CheckObj(child);
        }

        // Apply selection: closest hit object or null (deselect) if clicking on skybox/nothing
        LevelBrowser.SelectObject(closestObj);
    }
}
