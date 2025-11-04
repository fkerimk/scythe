using System.Numerics;
using Raylib_cs;

using SDColor = System.Drawing.Color;

namespace scythe;

#pragma warning disable CS8981
public static class colors {
    
    public static Color debug => SDColor.Red.to_color();
    public static Color primary => SDColor.OrangeRed.to_color();
    
    public static Color back => new(0.23f, 0.23f, 0.26f, 1f);
    public static Color game => new(0.1f, 0.1f, 0.11f, 1f);
    public static Color grid => new(0.18f, 0.18f, 0.18f, 1f);
    
    public static Color gui_text => SDColor.White.to_color();
    public static Color gui_text_disabled => SDColor.DimGray.to_color();
    public static Color gui_window_bg => new(0.15f, 0.16f, 0.17f, 1f);
    //ChildBg,
    //PopupBg,
    public static Color gui_border => SDColor.Black.to_color();
    //BorderShadow,
    //FrameBg,
    //FrameBgHovered,
    //FrameBgActive,
    public static Color gui_title_bg => new(0.1f, 0.11f, 0.12f, 1f);
    public static Color gui_title_bg_active => gui_title_bg;
    public static Color gui_title_bg_collapsed => gui_title_bg;
    //MenuBarBg,
    //ScrollbarBg,
    //ScrollbarGrab,
    //ScrollbarGrabHovered,
    //ScrollbarGrabActive,
    //CheckMark,
    //SliderGrab,
    //SliderGrabActive,
    //Button,
    //ButtonHovered,
    //ButtonActive,
    //Header,
    //HeaderHovered,
    //HeaderActive,
    //Separator,
    //SeparatorHovered,
    //SeparatorActive,
    public static Color gui_resize_grip => new (0, 0, 0, 0);
    public static Color gui_resize_grip_hovered => new(0.41f, 0.43f, 0.44f, 1f);
    public static Color gui_resize_grip_active => primary;
    public static Color gui_tab_hovered => new(0.34f, 0.36f, 0.37f, 1f);
    public static Color gui_tab => new(0.2f, 0.21f, 0.22f, 1f);
    public static Color gui_tab_selected => new(0.28f, 0.29f, 0.3f, 1f);
    public static Color gui_tab_selected_overline => gui_tab_selected;
    public static Color gui_tab_dimmed => gui_tab;
    public static Color gui_tab_dimmed_selected => gui_tab_dimmed;
    public static Color gui_tab_dimmed_selected_overline => gui_tab_dimmed;
    public static Color gui_docking_preview => primary;
    //DockingEmptyBg,
    //PlotLines,
    //PlotLinesHovered,
    //PlotHistogram,
    //PlotHistogramHovered,
    //TableHeaderBg,
    //TableBorderStrong,
    //TableBorderLight,
    //TableRowBg,
    //TableRowBgAlt,
    //TextLink,
    //TextSelectedBg,
    //DragDropTarget,
    //NavCursor,
    //NavWindowingHighlight,
    //NavWindowingDimBg,
    //ModalWindowDimBg,
    //COUNT,
    
    public static Color gui_frame => debug;
    
    public static uint file => uint_white;
    public static uint folder => uint_orange;

    public static Color to_color(this SDColor color) {
        
        return new(color.R, color.G, color.B, color.A);
    }
    
    public static Vector4 to_vector4(this Color color) {
        
        return new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
    
    public static uint to_uint(this Color color) {
        
        return @uint(color.R, color.G, color.B, color.A);
    }
    
    public static uint @uint(byte r, byte g, byte b, byte a = 255) {
        
        return ((uint)a << 24) | ((uint)b << 16) | ((uint)g << 8) | (uint)r;
    }
    
    public static uint uint_white => @uint(255, 255, 255);
    public static uint uint_gold => @uint(255, 215, 0);
    public static uint uint_orange => @uint(255, 165, 0);
    public static uint uint_yellow => @uint(255, 255, 0);
    public static uint uint_red => @uint(255, 0, 0);
    public static uint uint_blue =>@uint(0, 120, 215);
}