internal static class Colors {
    
    // generic
    public static Color Clear => new(0f, 0, 0, 0);
    public static Color Black => new(0, 0, 0);
    public static Color White => new(1, 1, 1);
    public static Color Yellow => new(1, 1, 0);
    public static Color Debug => new(1f, 0, 0);
    public static Color Primary => new(1f, 0.27f, 0f);
    public static Color PrimarySoft => new(0.7f, 0.3f, 0f);
    public static Color Back => new(0.23f, 0.23f, 0.26f);
    public static Color Game => new(0.1f, 0.1f, 0.11f);
    public static Color Grid => new(0.18f, 0.18f, 0.18f);
    
    // ImGui
    public static Color GuiText => White;
    public static Color GuiTextDisabled => new(0.41f, 0.41f, 0.41f);
    public static Color GuiWindowBg => new(0.15f, 0.16f, 0.17f);
    //ChildBg,
    //PopupBg,
    public static Color GuiBorder => new(0.1f, 0.1f, 0.11f);
    //BorderShadow,
    public static Color GuiFrameBg => PrimarySoft;
    public static Color GuiFrameBgHovered => Primary;
    public static Color GuiFrameBgActive => PrimarySoft;
    public static Color GuiTitleBg => new(0.1f, 0.11f, 0.12f);
    public static Color GuiTitleBgActive => GuiTitleBg;
    public static Color GuiTitleBgCollapsed => GuiTitleBg;
    //MenuBarBg,
    //ScrollbarBg,
    //ScrollbarGrab,
    //ScrollbarGrabHovered,
    //ScrollbarGrabActive,
    public static Color GuiCheckMark => GuiText;
    //SliderGrab,
    //SliderGrabActive,
    public static Color GuiButton => PrimarySoft;
    public static Color GuiButtonHovered => Primary;
    public static Color GuiButtonActive => PrimarySoft;
    public static Color GuiHeader => GuiWindowBg;
    public static Color GuiHeaderHovered => new(0.3f, 0.31f, 0.32f);
    public static Color GuiHeaderActive => Primary;
    //Separator,
    //SeparatorHovered,
    //SeparatorActive,
    public static Color GuiResizeGrip => Clear;
    public static Color GuiResizeGripHovered => new(0.41f, 0.43f, 0.44f);
    public static Color GuiResizeGripActive => Primary;
    public static Color GuiTabHovered => new(0.34f, 0.36f, 0.37f);
    public static Color GuiTab => new(0.2f, 0.21f, 0.22f);
    public static Color GuiTabSelected => new(0.28f, 0.29f, 0.3f);
    public static Color GuiTabSelectedOverline => GuiTabSelected;
    public static Color GuiTabDimmed => GuiTab;
    public static Color GuiTabDimmedSelected => GuiTabDimmed;
    public static Color GuiTabDimmedSelectedOverline => GuiTabDimmed;
    public static Color GuiDockingPreview => Primary;
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
    public static Color GuiDragDropTarget => Primary;
    //NavCursor,
    //NavWindowingHighlight,
    //NavWindowingDimBg,
    //ModalWindowDimBg,
    
    // gui
    public static Color GuiTreeEnabled => new(0.5f, 0.5f, 0.5f);
    public static Color GuiTreeDisabled => new(0.3f, 0.2f, 0.2f);
    public static Color GuiTreeSelected => PrimarySoft;
    public static Color GuiTypeObject    => new(0.5f, 0.5f, 0.5f);
    public static Color GuiTypeModel     => new(0.5f, 0.9f, 0.9f);
    public static Color GuiTypeTransform => new(0.9f, 0.5f, 0.2f);
    public static Color GuiTypeAnimation => new(0.9f, 0.5f, 0.9f);
    public static Color GuiTypeLight     => new(0.9f, 0.9f, 0.5f);
}