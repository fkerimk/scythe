namespace scythe;

#pragma warning disable CS8981
public static class colors {
    
    //
    public static color clear => new(0f, 0, 0, 0);
    public static color debug => new(1f, 0, 0);
    public static color primary => new(1f, 0.27f, 0f);
    
    //
    public static color back => new(0.23f, 0.23f, 0.26f);
    public static color game => new(0.1f, 0.1f, 0.11f);
    public static color grid => new(0.18f, 0.18f, 0.18f);
    
    //
    public static color gui_text => new color(1, 1, 1);
    public static color gui_text_disabled => new(0.41f, 0.41f, 0.41f);
    public static color gui_window_bg => new(0.15f, 0.16f, 0.17f);
    //ChildBg,
    //PopupBg,
    public static color gui_border => new(0.1f, 0.1f, 0.11f);
    //BorderShadow,
    //FrameBg,
    //FrameBgHovered,
    //FrameBgActive,
    public static color gui_title_bg => new(0.1f, 0.11f, 0.12f);
    public static color gui_title_bg_active => gui_title_bg;
    public static color gui_title_bg_collapsed => gui_title_bg;
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
    public static color gui_header => gui_window_bg;
    public static color gui_header_hovered => new(0.3f, 0.31f, 0.32f);
    public static color gui_header_active => primary;
    //Separator,
    //SeparatorHovered,
    //SeparatorActive,
    public static color gui_resize_grip => clear;
    public static color gui_resize_grip_hovered => new(0.41f, 0.43f, 0.44f);
    public static color gui_resize_grip_active => primary;
    public static color gui_tab_hovered => new(0.34f, 0.36f, 0.37f);
    public static color gui_tab => new(0.2f, 0.21f, 0.22f);
    public static color gui_tab_selected => new(0.28f, 0.29f, 0.3f);
    public static color gui_tab_selected_overline => gui_tab_selected;
    public static color gui_tab_dimmed => gui_tab;
    public static color gui_tab_dimmed_selected => gui_tab_dimmed;
    public static color gui_tab_dimmed_selected_overline => gui_tab_dimmed;
    public static color gui_docking_preview => primary;
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
    public static color gui_drag_drop_target => primary;
    //NavCursor,
    //NavWindowingHighlight,
    //NavWindowingDimBg,
    //ModalWindowDimBg,
    //COUNT,
    
    //
    public static color gui_tree_enabled => new(0.5f, 0.5f, 0.5f);
    public static color gui_tree_disabled => new(0.3f, 0.2f, 0.2f);
    
    public static color gui_type_object    => new(0.5f, 0.5f, 0.5f);
    public static color gui_type_model     => new(0.5f, 0.9f, 0.9f);
    public static color gui_type_transform => new(0.9f, 0.5f, 0.2f);
    public static color gui_type_animation => new(0.9f, 0.5f, 0.9f);
    
    //public static color file => uint_white;
    //public static color folder => uint_orange;
}