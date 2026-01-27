internal class ScriptAsset : Asset {

    public string Content = "";

    public override bool Load() {

        if (!System.IO.File.Exists(File)) return false;

        try {

            Content  = System.IO.File.ReadAllText(File);
            IsLoaded = true;

            return true;
        } catch {
            return false;
        }
    }

    public override void Unload() {

        Content  = "";
        IsLoaded = false;
    }
}