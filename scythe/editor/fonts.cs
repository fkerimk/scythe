using System.Runtime.InteropServices;
using ImGuiNET;
using rlImGui_cs;

namespace scythe;

#pragma warning disable CS8981
public static class fonts {
    
    private const int normal_size = 16;
    private const int large_size = 32;
    
    public static ImFontPtr montserrat_regular;
    public static ImFontPtr font_awesome_normal;
    public static ImFontPtr font_awesome_large;

    public static unsafe void load(ImGuiIOPtr imgui) {
        
        var config = ImGuiNative.ImFontConfig_ImFontConfig();
        config->OversampleH = 3;
        config->OversampleV = 3;
        
        var icon_ranges = GCHandle.Alloc(new ushort[] { 0xE000, 0xF8FF, 0 }, GCHandleType.Pinned).AddrOfPinnedObject();
        
        montserrat_regular = imgui.Fonts.AddFontFromFileTTF(path.relative("font/montserrat-regular.otf"), normal_size, config);
        font_awesome_normal = imgui.Fonts.AddFontFromFileTTF(path.relative("font/fa7-free-solid.otf"), normal_size, config, icon_ranges);
        font_awesome_large = imgui.Fonts.AddFontFromFileTTF(path.relative("font/fa7-free-solid.otf"), large_size, config, icon_ranges);
        
        rlImGui.ReloadFonts();
    }
}