internal static class Colors {
    
    // generic
    public static ScytheColor Clear => new(0f, 0, 0, 0);
    public static ScytheColor Black => new(0, 0, 0);
    public static ScytheColor White => new(1, 1, 1);
    public static ScytheColor Yellow => new(1, 1, 0);
    public static ScytheColor Debug => new(1f, 0, 0);
    public static ScytheColor Primary => new(1f, 0.27f, 0f);
    private static ScytheColor PrimarySoft => new(0.7f, 0.3f, 0f);
    public static ScytheColor Back => new(0.23f, 0.23f, 0.26f);
    public static ScytheColor Game => new(0.1f, 0.1f, 0.11f);
    public static ScytheColor Grid => new(0.18f, 0.18f, 0.18f);
    
    // ImGui
    public static ScytheColor GuiText => White;
    public static ScytheColor GuiTextDisabled => new(0.41f, 0.41f, 0.41f);
    public static ScytheColor GuiWindowBg => new(0.15f, 0.16f, 0.17f);
    //ChildBg,
    //PopupBg,
    public static ScytheColor GuiBorder => new(0.1f, 0.1f, 0.11f);
    //BorderShadow,
    public static ScytheColor GuiFrameBg => PrimarySoft;
    public static ScytheColor GuiFrameBgHovered => Primary;
    public static ScytheColor GuiFrameBgActive => PrimarySoft;
    public static ScytheColor GuiTitleBg => new(0.1f, 0.11f, 0.12f);
    public static ScytheColor GuiTitleBgActive => GuiTitleBg;
    public static ScytheColor GuiTitleBgCollapsed => GuiTitleBg;
    //MenuBarBg,
    //ScrollbarBg,
    //ScrollbarGrab,
    //ScrollbarGrabHovered,
    //ScrollbarGrabActive,
    public static ScytheColor GuiCheckMark => GuiText;
    //SliderGrab,
    //SliderGrabActive,
    public static ScytheColor GuiButton => PrimarySoft;
    public static ScytheColor GuiButtonHovered => Primary;
    public static ScytheColor GuiButtonActive => PrimarySoft;
    public static ScytheColor GuiHeader => GuiWindowBg;
    public static ScytheColor GuiHeaderHovered => new(0.3f, 0.31f, 0.32f);
    public static ScytheColor GuiHeaderActive => Primary;
    //Separator,
    //SeparatorHovered,
    //SeparatorActive,
    public static ScytheColor GuiResizeGrip => Clear;
    public static ScytheColor GuiResizeGripHovered => new(0.41f, 0.43f, 0.44f);
    public static ScytheColor GuiResizeGripActive => Primary;
    public static ScytheColor GuiTabHovered => new(0.34f, 0.36f, 0.37f);
    public static ScytheColor GuiTab => new(0.2f, 0.21f, 0.22f);
    public static ScytheColor GuiTabSelected => new(0.28f, 0.29f, 0.3f);
    public static ScytheColor GuiTabSelectedOverline => GuiTabSelected;
    public static ScytheColor GuiTabDimmed => GuiTab;
    public static ScytheColor GuiTabDimmedSelected => GuiTabDimmed;
    public static ScytheColor GuiTabDimmedSelectedOverline => GuiTabDimmed;
    public static ScytheColor GuiDockingPreview => Primary;
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
    public static ScytheColor GuiDragDropTarget => Primary;
    //NavCursor,
    //NavWindowingHighlight,
    //NavWindowingDimBg,
    //ModalWindowDimBg,
    
    // gui
    public static ScytheColor GuiTreeEnabled => new(0.5f, 0.5f, 0.5f);
    public static ScytheColor GuiTreeDisabled => new(0.3f, 0.2f, 0.2f);
    public static ScytheColor GuiTreeSelected => PrimarySoft;
    public static ScytheColor GuiTypeObject    => new(0.5f, 0.5f, 0.5f);
    public static ScytheColor GuiTypeModel     => new(0.5f, 0.9f, 0.9f);
    public static ScytheColor GuiTypeTransform => new(0.9f, 0.5f, 0.2f);
    public static ScytheColor GuiTypeAnimation => new(0.9f, 0.5f, 0.9f);
    public static ScytheColor GuiTypeLight     => new(0.9f, 0.9f, 0.5f);
    public static ScytheColor GuiTypeCamera    => new(0.5f, 0.9f, 0.9f);
}