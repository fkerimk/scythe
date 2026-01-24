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

        // Thumbnail (Resize copy)
        var w = image.Width;
        var h = image.Height;
        int newW, newH;
        
        if (w > h) {
            
            newW = 64;
            newH = (int)((float)h / w * 64);
            
        } else {
            
            newH = 64;
            newW = (int)((float)w / h * 64);
        }
        
        var thumbImage = ImageCopy(image);
        ImageResize(&thumbImage, newW, newH);
        Thumbnail = LoadTextureFromImage(thumbImage);
        UnloadImage(thumbImage);

        UnloadImage(image);

        IsLoaded = true;
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
