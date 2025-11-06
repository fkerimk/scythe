namespace scythe;

#pragma warning disable CS8981
public class level {

    public obj root;

    public level() {

        root = new("root", "object");

        var cube = new obj("cube", "object", root);
        cube.set_parent(root);
        
        var cube_t = new obj("transform", "transform", cube);
        cube_t.set_parent(cube);
        (cube_t.type_class as transform)!.pos = new(0, -0.1f, 0);
        (cube_t.type_class as transform)!.scale = new(3, 0.2f, 3);

        var cube_m = new obj("model", "model", cube);
        cube_m.set_parent(cube);
        (cube_m.type_class as model)!.path = "model/cube/cube.glb";
        
        var bear_man = new obj("bear_man", "object", root);
        bear_man.set_parent(root);
        
        var bear_man_t = new obj("transform", "transform", cube);
        bear_man_t.set_parent(bear_man);
   
        var bear_man_m = new obj("model", "model", cube);
        bear_man_m.set_parent(bear_man);
        (bear_man_m.type_class as model)!.path = "model/bear_man/bear_man.glb";
        
        var bear_man_a = new obj("animation", "animation", cube);
        bear_man_a.set_parent(bear_man_m);
        (bear_man_a.type_class as animation)!.path = "model/bear_man/bear_man.glb";
        (bear_man_a.type_class as animation)!.track = 1;


        //for (var i = 0; i < 32; i++) {
        //    
        //    var child = new obj($"child {i}", "object", root);
        //    root.children.Add(child);
        //}
    }
}