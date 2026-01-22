using Raylib_cs;
using static Raylib_cs.Raylib;

internal unsafe class AnimationAsset : Asset {
    
    public ModelAnimation* Animations;
    public int AnimationCount = -1;
    
    public override bool Load() {
        
        if (Path.GetExtension(File) != ".iqm" || !System.IO.File.Exists(File)) return false;
        
        Animations = LoadModelAnimations(File, ref AnimationCount);
        IsLoaded = true;
        
        return true;
    }

    public override void Unload() => UnloadModelAnimations(Animations, AnimationCount);
}