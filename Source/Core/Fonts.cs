using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

internal static class Fonts {
    
    private const int
        SmallSize = 11,
        NormalSize = 16,
        LargeSize = 32;
    
    public static ImFontPtr ImMontserratRegular;  public static Font RlMontserratRegular;
    public static ImFontPtr ImFontAwesomeSmall ;
    public static ImFontPtr ImFontAwesomeNormal;
    public static ImFontPtr ImFontAwesomeLarge ;

    private static ImFontConfigPtr _imFontConfigPtr;
    private static IntPtr _iconRanges;
    
    public static unsafe void Init() {
        
        if (CommandLine.Editor) {
        
            _iconRanges = GCHandle.Alloc(new ushort[] { 0xE000, 0xF8FF, 0 }, GCHandleType.Pinned).AddrOfPinnedObject();
        
            _imFontConfigPtr = ImGuiNative.ImFontConfig_ImFontConfig();
            _imFontConfigPtr.OversampleH = 3;
            _imFontConfigPtr.OversampleV = 3;
            
            ImMontserratRegular = LoadFont<ImFontPtr>("Fonts/montserrat-regular.otf");
            ImFontAwesomeSmall  = LoadFont<ImFontPtr>("Fonts/fa7-free-solid.otf", SmallSize , true);
            ImFontAwesomeNormal = LoadFont<ImFontPtr>("Fonts/fa7-free-solid.otf", NormalSize, true);
            ImFontAwesomeLarge  = LoadFont<ImFontPtr>("Fonts/fa7-free-solid.otf", LargeSize , true);
            
            rlImGui.ReloadFonts();
        }
        
        RlMontserratRegular = LoadFont<Font>("Fonts/montserrat-regular.otf");
    }
    
    public static void UnloadRlFonts() {
        
        Raylib.UnloadFont(RlMontserratRegular);
    }

    private static T LoadFont<T>(string path, int size = NormalSize, bool useIconRanges = false) {

        if (!PathUtil.BestPath(path, out var bestPath)) throw new FileNotFoundException($"Font {path} not found");

        if (typeof(T) == typeof(Font)) {

            return (T)(object)Raylib.LoadFont(bestPath) ;
        }
        
        if (typeof(T) == typeof(ImFontPtr)) {

            return (T)(object) (useIconRanges ?
                Editor.ImGuiIoPtr.Fonts.AddFontFromFileTTF(bestPath, size, _imFontConfigPtr, _iconRanges) :
                Editor.ImGuiIoPtr.Fonts.AddFontFromFileTTF(bestPath, size, _imFontConfigPtr));
        }

        throw new NotSupportedException($"Unsupported font type: {typeof(T)}");
    }
}