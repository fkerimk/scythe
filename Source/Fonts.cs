using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

internal static class Fonts {
    
    private const int SmallSize = 11;
    private const int NormalSize = 16;
    private const int LargeSize = 32;
    
    public static ImFontPtr ImMontserratRegular;
    public static Font RlMontserratRegular;
    
    public static ImFontPtr ImFontAwesomeSmall;
    public static ImFontPtr ImFontAwesomeNormal;
    public static ImFontPtr ImFontAwesomeLarge;

    public static unsafe void LoadImFonts(ImGuiIOPtr imGui) {
        
        var config = ImGuiNative.ImFontConfig_ImFontConfig();
        config->OversampleH = 3;
        config->OversampleV = 3;
        
        var iconRanges = GCHandle.Alloc(new ushort[] { 0xE000, 0xF8FF, 0 }, GCHandleType.Pinned).AddrOfPinnedObject();
        
        ImMontserratRegular = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/montserrat-regular.otf"), NormalSize, config);
        ImFontAwesomeSmall  = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/fa7-free-solid.otf"), SmallSize, config, iconRanges);
        ImFontAwesomeNormal = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/fa7-free-solid.otf"), NormalSize, config, iconRanges);
        ImFontAwesomeLarge  = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/fa7-free-solid.otf"), LargeSize, config, iconRanges);
        
        rlImGui.ReloadFonts();
    }

    public static void LoadRlFonts() {

        RlMontserratRegular = Raylib.LoadFont(PathUtil.Relative("Fonts/montserrat-regular.otf"));
    }

    public static void UnloadRlFonts() {
        
        Raylib.UnloadFont(RlMontserratRegular);
    }
}