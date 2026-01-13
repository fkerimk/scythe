using System.Runtime.InteropServices;
using ImGuiNET;
using rlImGui_cs;

internal static class Fonts {
    
    private const int SmallSize = 11;
    private const int NormalSize = 16;
    private const int LargeSize = 32;
    
    public static ImFontPtr MontserratRegular;
    public static ImFontPtr FontAwesomeSmall;
    public static ImFontPtr FontAwesomeNormal;
    public static ImFontPtr FontAwesomeLarge;

    public static unsafe void Load(ImGuiIOPtr imGui) {
        
        var config = ImGuiNative.ImFontConfig_ImFontConfig();
        config->OversampleH = 3;
        config->OversampleV = 3;
        
        var iconRanges = GCHandle.Alloc(new ushort[] { 0xE000, 0xF8FF, 0 }, GCHandleType.Pinned).AddrOfPinnedObject();
        
        MontserratRegular = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/montserrat-regular.otf"), NormalSize, config);
        FontAwesomeSmall  = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/fa7-free-solid.otf"), SmallSize, config, iconRanges);
        FontAwesomeNormal = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/fa7-free-solid.otf"), NormalSize, config, iconRanges);
        FontAwesomeLarge  = imGui.Fonts.AddFontFromFileTTF(PathUtil.Relative("Fonts/fa7-free-solid.otf"), LargeSize, config, iconRanges);
        
        rlImGui.ReloadFonts();
    }
}