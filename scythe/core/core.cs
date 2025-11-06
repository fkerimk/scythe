using System.Numerics;

namespace scythe;

#pragma warning disable CS8981 
public class core {

    public cam cam;
    public level level;

    public core() {

        cam = new();
        level = new();
    }

    public void loop_3d(bool is_editor) {
        
        loop_3d_obj(level.root, is_editor);
    }
    
    private void loop_3d_obj(obj obj, bool is_editor, int index = 0) {
        
        obj.matrix = Matrix4x4.Identity;
        obj.type_class?.loop_3d(is_editor);

        foreach (var priority in new[] { 0, 1, 2 }) {
            
            foreach (var child in from child in obj.children let sortOrder = child.type_class switch {
                         
                transform => 0,
                animation => 1,
                model => 1,
                _ => 2

            } where sortOrder == priority select child) loop_3d_obj(child, is_editor, index + 1);
        }
    }

    public void loop_ui(bool is_editor) {

        loop_ui_obj(level.root, is_editor);
    }
    
    private void loop_ui_obj(obj obj, bool is_editor, int index = 0) {
        
        obj.type_class?.loop_ui(is_editor);
        
        foreach (var child in obj.children)
            loop_ui_obj(child, is_editor, index + 1);
    }

    public void loop_editor(viewport viewport) {

        loop_editor_obj(level.root, viewport);
    }
    
    private void loop_editor_obj(obj obj, viewport viewport, int index = 0) {
        
        obj.type_class?.loop_editor(viewport);
        
        foreach (var child in obj.children)
            loop_editor_obj(child, viewport, index + 1);
    }

    public void quit() {
        
        quit_obj(level.root);
    }

    private void quit_obj(obj obj, int index = 0) {
        
        obj.type_class?.quit();
        
        foreach (var child in obj.children)
             quit_obj(child, index + 1);
    }
}