internal class AnimationAsset : Asset {

    public List<AnimationClip> Animations = [];

    public override bool Load() {

        if (!System.IO.File.Exists(File)) return false;

        try {

            var data = AssimpLoader.Load(File);
            Animations = data.Animations;

        } catch {

            return false;
        }

        IsLoaded = true;

        return true;
    }

    public override void Unload() => Animations.Clear();
}