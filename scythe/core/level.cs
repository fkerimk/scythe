using System.Reflection;

namespace scythe;

#pragma warning disable CS8981
#pragma warning disable IL2067
#pragma warning disable IL2026
public class level {

    public readonly obj root;

    public level() {

        root = new("root", typeof(obj));

        {
            var obj = make_object("cube",  root);
            var transform = make_object("transform", obj, "transform");
            (transform.type as transform)!.scale = new(2, 0.2f, 2);
            var model = make_object("model", obj, "model");
            (model.type as model)!.path = "model/cube.glb";
        }

        {
            var obj = make_object("bear_man", root);
            make_object("transform", obj, "transform");
            var model = make_object("model", obj, "model");
            (model.type as model)!.path = "model/bear_man.glb";
            var animation = make_object("animation", model, "animation");
            (animation.type as animation)!.path = "model/bear_man.glb";
            (animation.type as animation)!.track = 2;
        }
        
        { 
            var obj = make_object("point_light", root); 
            var light = make_object("light", obj, "light");
            (light.type as light)!.intensity = 5;
            var transform = make_object("transform", obj, "transform");
            (transform.type as transform)!.pos = new(0, 3, 1);
        }
    }
    
    public obj make_object(string name, obj parent, string? type = null) {

        var type_class = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Namespace == "scythe" && t.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
        
        var obj = new obj(name, type_class, parent);
        obj.set_parent(parent);

        return obj;
    }
}