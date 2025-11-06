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

    public void loop_3d() {
        
        loop_obj(level.root);
    }

    public void loop_ui() {
        
        
    }

    public void loop_obj(obj obj, int index = 0) {
        
        obj.matrix = Matrix4x4.Identity;
        obj.type_class?.loop();

        foreach (var priority in new[] { 0, 1, 2 }) {
            
            foreach (var child in from child in obj.children let sortOrder = child.type_class switch {
                         
                transform => 0,
                animation => 1,
                model => 1,
                _ => 2
                
            } where sortOrder == priority select child) loop_obj(child, index + 1);
        }
    }
}