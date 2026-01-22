using Raylib_cs;

internal static class Picking {

    public static unsafe void Update() {

        if (Core.ActiveCamera == null || Core.ActiveLevel == null) return;
        
        // Ensure viewport interaction is valid and prevent picking when clicking UI
        if (!Editor.EditorRender.IsHovered) return;
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return;

        // Prevent picking if gizmo is interacting
        if (LevelBrowser.SelectedObject?.Transform.IsHovered == true || LevelBrowser.SelectedObject?.Transform.IsDragging == true) return;

        var ray = Raylib.GetScreenToWorldRay(EditorRender.RelativeMouse3D, Core.ActiveCamera.Raylib);
        
        Obj? closestObj = null;
        var minDistance = float.MaxValue;

        // Start recursive search from the root of the active level
        CheckObj(Core.ActiveLevel.Root);

        // Apply selection: closest hit object or null (deselect) if clicking on skybox/nothing
        LevelBrowser.SelectObject(closestObj);
        
        return;

        void CheckObj(Obj obj) {

            foreach (var component in obj.Components.Values) {

                // Only perform collision check if the model is loaded
                if (component is not Model { IsLoaded: true } model) continue;
                
                for (var i = 0; i < model.Asset.RlModel.MeshCount; i++) {
                        
                    var collision = Raylib.GetRayCollisionMesh(ray, model.Asset.RlModel.Meshes[i], obj.WorldMatrix);

                    if (!collision.Hit || !(collision.Distance < minDistance)) continue;
                    
                    minDistance = collision.Distance;
                    closestObj = obj;
                }
            }

            // Traverse children recursively
            foreach (var child in obj.Children.Values)
                CheckObj(child);
        }
    }
}
