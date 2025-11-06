using Raylib_cs;

namespace scythe;

#pragma warning disable CS8981 
public class cam {

    public Camera3D rl_cam;
    
    //public float3 up {
    //    get => rl_cam.Up.to_float3();
    //    set => rl_cam.Up = value.to_vector3();
    //}
    
    public float3 pos {
        get => rl_cam.Position.to_float3();
        set => rl_cam.Position = value.to_vector3();
    }
    
    public float3 target {
        get => rl_cam.Target.to_float3();
        set => rl_cam.Target = value.to_vector3();
    }
    
    public float fov {
        get => rl_cam.FovY;
        set => rl_cam.FovY = value;
    }
    
    public float3 up => -float3.normalize(float3.cross(fwd, right));
    public float3 fwd => float3.normalize(target - pos);
    public float3 right => float3.normalize(float3.cross(fwd, float3.up));

    public cam() {
        
        rl_cam = new() {
            
            Projection = CameraProjection.Perspective,
            FovY = 90,
            Position = new(6, 6, 6),
            Up = new(0, 1, 0)
        };
    }

    public void start_rendering() {
        
        Raylib.BeginMode3D(rl_cam);
    }

    public void stop_rendering() {
        
        Raylib.EndMode3D();
    }
}