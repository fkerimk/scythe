using System.Numerics;
using Raylib_cs;
using Newtonsoft.Json;

internal class Camera(Obj obj) : Component(obj) {

    public override string LabelIcon => Icons.FaVideoCamera;
    public override Color LabelColor => Colors.GuiTypeCamera;

    public required Camera3D Cam = new();
    
    [RecordHistory, JsonProperty, Label("FOV")] public float Fov { get => Cam.FovY; set => Cam.FovY = value; }
    [RecordHistory, JsonProperty, Label("Near Clip")] public float NearClip { get; set; } = 0.01f;
    [RecordHistory, JsonProperty, Label("Far Clip")] public float FarClip { get; set; } = 1000.0f;

    public override void Logic() {
        
        Obj.DecomposeWorldMatrix(out var pos, out var rot, out _);
        
        var forward = Vector3.Transform(Vector3.UnitZ, rot);

        Cam.Position = pos;
        Cam.Target = (pos + forward);
        Cam.Up = Vector3.Transform(Vector3.UnitY, rot);
    }

    public static void ApplySettings(Camera3D cam, float near, float far) {
        
         Rlgl.SetClipPlanes(near, far);
    }

    public override void Render3D() {

        if (!CommandLine.Editor || Core.IsPlaying || Core.IsPreviewRender) return;
        
        Cam.Raylib.DrawCameraFrustum(Raylib.ColorAlpha(Color.White, IsSelected ? 1 : 0.3f), Fov, NearClip, FarClip);
    }
}