using System.Numerics;
using Raylib_cs;

internal class Cam {

    public Camera3D RlCam = new() {
            
        Projection = CameraProjection.Perspective,
        FovY = 90,
        Position = new Vector3(2, 2, 2),
        Target = new Vector3(0, 1, 0),
        Up = new Vector3(0, 1, 0)
    };
    
    public float3 Pos {
        get => RlCam.Position.to_float3();
        set => RlCam.Position = value.to_vector3();
    }
    
    public float3 Target {
        get => RlCam.Target.to_float3();
        set => RlCam.Target = value.to_vector3();
    }
    
    public float Fov {
        get => RlCam.FovY;
        set => RlCam.FovY = value;
    }
    
    public float3 Up => -float3.normalize(float3.cross(Fwd, Right));
    public float3 Fwd => float3.normalize(Target - Pos);
    public float3 Right => float3.normalize(float3.cross(Fwd, float3.up));

    public void StartRendering() {
        
        Raylib.BeginMode3D(RlCam);
    }

    public void StopRendering() {
        
        Raylib.EndMode3D();
    }
}