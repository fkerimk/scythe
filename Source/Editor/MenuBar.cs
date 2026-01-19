using static ImGuiNET.ImGui;

internal static class MenuBar {

    public static void Draw() {
        
        if (!BeginMainMenuBar()) return;
        
        if (BeginMenu("File")) {
                
            if (MenuItem("New", "Ctrl+N", false, false)) { }
            if (MenuItem("Open", "Ctrl+O", false, false)) { }
            Separator();
            if (MenuItem("Exit", "Ctrl+Q")) Editor.Quit();
            EndMenu();
        }

        if (BeginMenu("Edit")) {
                
            if (MenuItem("Delete", "Del", false, LevelBrowser.CanDeleteSelectedObject)) LevelBrowser.DeleteSelectedObject();
            Separator();
            if (MenuItem("Undo", "Ctrl+Z", false, History.CanUndo)) History.Undo();
            if (MenuItem("Redo", "Ctrl+Y", false, History.CanRedo)) History.Redo();
                
            EndMenu();
        }

        if (BeginMenu("View")) {
            
            MenuItem("3D Viewport", "", ref Editor.Level3D.IsOpen);
            MenuItem("Level Browser", "", ref Editor.LevelBrowser.IsOpen);
            MenuItem("Object Browser", "", ref Editor.ObjectBrowser.IsOpen);
            MenuItem("Project Browser", "", ref Editor.ProjectBrowser.IsOpen);
            
            EndMenu();
        }

        EndMainMenuBar();
    }
}