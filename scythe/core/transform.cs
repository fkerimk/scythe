using System.Numerics;
using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981
public class transform(obj obj) : type(obj) {
    
    public override string label_icon => icons.transform;
    public override color label_color => colors.gui_type_transform;
    
    [label("Pos")] public float3 pos { get; set; } = float3.zero;
    
    [label("Euler")] public float3 euler { 
        
        get => Raymath.QuaternionToEuler(rot).to_float3().to_deg();
        set => rot = Raymath.QuaternionFromEuler(value.x.to_rad(), value.y.to_rad(), value.z.to_rad());
    }
    
    [label("Scale")] public float3 scale { get; set; } = float3.one;
    
    private Quaternion rot { get; set; } = Quaternion.Identity;

    private int mode;
    private float active_move;
    
    private string active_id = "";
    private Vector2 active_mouse_temp;
    private float3 active_pos;
    private float3 active_scale;
    private Quaternion active_rot = Quaternion.Identity;
    private float3 active_normal;

    private const float move_snap = 0.2f;
    
    public override void loop_3d(bool is_editor) {

        if (obj.parent == null) return;
        
        obj.parent.rot_matrix = Matrix4x4.CreateFromQuaternion(rot);
        
        obj.parent.matrix = Raymath.MatrixMultiply(

            Raymath.MatrixMultiply(
    
                Raymath.MatrixScale(scale.x, scale.y, scale.z),
                Matrix4x4.Transpose(obj.parent.rot_matrix)
            ),

            Raymath.MatrixTranslate(pos.x, pos.y, pos.z)
        );
        
    }

    public override void loop_ui(bool is_editor) {}

    public override void loop_3d_editor(viewport viewport) {

        if (cam.main == null) return;

        if (obj.parent == null || (!obj.parent.is_selected && !obj.is_selected && !obj.parent.children.Any(_obj => _obj.is_selected))) return;
        
        if (Raylib.IsKeyPressed(KeyboardKey.One)) mode = 0;
        if (Raylib.IsKeyPressed(KeyboardKey.Two)) mode = 1;
        if (Raylib.IsKeyPressed(KeyboardKey.Three)) mode = 2;
        
        shaders.begin(shaders.transform);

        var ray = Raylib.GetScreenToWorldRay(viewport.relative_mouse_3d, cam.main.rl_cam);
        //Raylib.DrawSphere(ray.Position + ray.Direction * 15, 0.1f, Color.Magenta);
        
        axis("x", obj.parent.right, new(0.9f, 0.3f, 0.3f), ray);
        axis("y", obj.parent.up, new(0.3f, 0.9f, 0.3f), ray);
        axis("z", obj.parent.fwd, new(0.3f, 0.3f, 0.9f), ray);
        
        shaders.end();
    }

    public override void loop_ui_editor(viewport viewport) {
        
        if (Raylib.IsCursorHidden()) return;
        
        if (obj.parent == null || (!obj.parent.is_selected && !obj.is_selected && !obj.parent.children.Any(_obj => _obj.is_selected))) return;
        
        var text_a = mode switch { 0 => "pos", 1 => "rot", 2 => "scale", _ => "bruh" };
        var text_pos_a = new int2(viewport.relative_mouse.X, viewport.relative_mouse.Y - 15);
        
        Raylib.DrawText(text_a, text_pos_a.x - 14, text_pos_a.y - 19, 20, colors.black.to_raylib());
        Raylib.DrawText(text_a, text_pos_a.x - 15, text_pos_a.y - 20, 20, colors.yellow.to_raylib());
        
        if (active_move == 0) return;
        
        var text_b = mode switch { 0 or 2 => $"{active_move:F2}m", 1 => $"{active_move:F2}°", _ => $"{active_move:F2}" }; 
        var text_pos_b = new int2(viewport.relative_mouse.X, viewport.relative_mouse.Y - 15);
            
        Raylib.DrawText(text_b, text_pos_b.x - 14, text_pos_b.y - 39, 20, colors.black.to_raylib());
        Raylib.DrawText(text_b, text_pos_b.x - 15, text_pos_b.y - 40, 20, colors.yellow.to_raylib());
    }

    private void axis(string id, float3 normal, color axis_color, Ray ray) {

        if (cam.main == null) return;
        
        var is_active = active_id == id;
        
        var a = pos + (Vector3.Normalize(normal.to_vector3()) * 0.1f).to_float3();
        var b = a + normal * 1.5f;

        if (!string.IsNullOrEmpty(active_id) && active_id != id) {
            
            Raylib.DrawLine3D(a.to_vector3(), b.to_vector3(), axis_color.to_raylib());
            return;
        }

        if (is_active) {

            var new_pos = pos;
            var new_rot = rot;
            var new_scale = scale;

            var diff = (Raylib.GetMousePosition() - active_mouse_temp) * mode switch { 0 => 0.01f, 1 => 1f, 2 => 0.05f, _ => 0 };
                
            var drag = mode switch {
                
                0 or 2 => cam.main.right * diff.X + cam.main.up * -diff.Y,
                1 => cam.main.up * diff.X + cam.main.right * diff.Y,
                _ => float3.zero
            };

            var move = Vector3.Dot(drag.to_vector3(), active_normal.to_vector3());

            active_move = mode switch {
                    
                0 when Raylib.IsKeyDown(KeyboardKey.LeftShift) => MathF.Round(move / move_snap) * move_snap,
                1 when Raylib.IsKeyDown(KeyboardKey.LeftShift) => MathF.Round(move / 22.5f) * 22.5f,
                2 when Raylib.IsKeyDown(KeyboardKey.LeftShift) => MathF.Round(move / move_snap) * move_snap,
                _ => move
            };
            
            switch (mode) {
                
                case 0: new_pos = active_pos + active_normal * active_move; break;
                
                case 1:
                    var normal_q = Quaternion.CreateFromAxisAngle(active_normal.to_vector3(), active_move.to_rad());
                    new_rot = normal_q * active_rot;
                    break;
                
                case 2: new_scale = active_scale + active_normal * active_move; break;
            }
            
            pos = new_pos;
            rot = new_rot;
            scale = new_scale;
        }
        
        const float radius = 0.025f, ray_radius = 0.25f;
        const int ray_quality = 9;
        
        var is_hovered = false;
        
        for (var i = 0; i < ray_quality + 1; i++) {

            var step = Vector3.Lerp(a.to_vector3(), b.to_vector3(), 1f / ray_quality * i).to_float3();

            if (Raylib.GetRayCollisionSphere(ray, step.to_vector3(), ray_radius).Hit)
                is_hovered = true;
        }
        
        if (is_hovered && Raylib.IsMouseButtonPressed(MouseButton.Left)) {
            
            active_id = id;

            active_pos = pos;
            active_rot = rot;
            active_scale = scale;
    
            active_normal = normal;
            active_mouse_temp = Raylib.GetMousePosition();

            active_normal = mode switch {

                2 => id switch {
                    
                    "x" => new(1, 0, 0),
                    "y" => new(0, 1, 0),
                    "z" => new(0, 0, 1),
                    _ => float3.zero
                },
                    
                _ => normal
            };
        }

        if ((is_active && Raylib.IsMouseButtonReleased(MouseButton.Left)) || Raylib.IsCursorHidden()) {
            
            active_id = "";
            active_move = 0;
        }

        var target_color = (!is_active && is_hovered && !Raylib.IsCursorHidden()) ? colors.white : axis_color;
        
        Raylib.DrawCylinderEx(a.to_vector3(), b.to_vector3(), radius, radius, 1, target_color.to_raylib());
    }
    
    public override void quit() {}
}