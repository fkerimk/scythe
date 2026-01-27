using Raylib_cs;
using static Raylib_cs.Raylib;

internal class TextureAsset : Asset {

    public Texture2D Texture;

    public override unsafe bool Load() {

        if (!System.IO.File.Exists(File)) return false;

        var image = LoadImage(File);

        if (image.Data == null) return false;

        // Main Texture
        Texture = LoadTextureFromImage(image);
        SetTextureFilter(Texture, TextureFilter.Bilinear);
        UnloadImage(image);

        IsLoaded = true;
        Preview.UpdateThumbnail(this);

        return true;
    }

    public override void Unload() {

        if (IsLoaded) {

            UnloadTexture(Texture);
            if (Thumbnail.HasValue) UnloadTexture(Thumbnail.Value);
        }

        IsLoaded = false;
    }
}