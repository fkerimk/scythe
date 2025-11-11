using System.Numerics;
using ImGuiNET;

namespace scythe;

#pragma warning disable CS8981
public static class style {

    private static int pushes;
    private static readonly List<int> pushed_colors = [];
    private static readonly List<int> pushed_vars = [];
    
    public static void push(custom_style? style = null) {

        if (pushed_colors.Count == pushes) pushed_colors.Add(0);
        if (pushed_vars.Count == pushes) pushed_vars.Add(0);
        
        //Console.WriteLine( ImGui.GetStyle().TouchExtraPadding); 
        
        set(ImGuiCol.Text, colors.gui_text);
        set(ImGuiCol.TextDisabled, colors.gui_text_disabled);
        set(ImGuiCol.WindowBg, colors.gui_window_bg);
        //ChildBg,
        //PopupBg,
        set(ImGuiCol.Border, colors.gui_border);
        //BorderShadow,
        set(ImGuiCol.FrameBg, colors.gui_frame_bg);
        set(ImGuiCol.FrameBgHovered, colors.gui_frame_bg_hovered);
        set(ImGuiCol.FrameBgActive, colors.gui_frame_bg_active);
        set(ImGuiCol.TitleBg, colors.gui_title_bg);
        set(ImGuiCol.TitleBgActive, colors.gui_title_bg_active);
        set(ImGuiCol.TitleBgCollapsed, colors.gui_title_bg_collapsed);
        //MenuBarBg,
        //ScrollbarBg,
        //ScrollbarGrab,
        //ScrollbarGrabHovered,
        //ScrollbarGrabActive,
        set(ImGuiCol.CheckMark, colors.gui_check_mark);
        //SliderGrab,
        //SliderGrabActive,
        set(ImGuiCol.Button, colors.gui_button);
        set(ImGuiCol.ButtonHovered, colors.gui_button_hovered);
        set(ImGuiCol.ButtonActive, colors.gui_button_active);
        set(ImGuiCol.Header, colors.gui_header);
        set(ImGuiCol.HeaderHovered, colors.gui_header_hovered);
        set(ImGuiCol.HeaderActive, colors.gui_header_active);
        //Separator,
        //SeparatorHovered,
        //SeparatorActive,
        set(ImGuiCol.ResizeGrip, colors.gui_resize_grip);
        set(ImGuiCol.ResizeGripHovered, colors.gui_resize_grip_hovered);
        set(ImGuiCol.ResizeGripActive, colors.gui_resize_grip_active);
        set(ImGuiCol.TabHovered, colors.gui_tab_hovered);
        set(ImGuiCol.Tab, colors.gui_tab);
        set(ImGuiCol.TabSelected, colors.gui_tab_selected);
        set(ImGuiCol.TabSelectedOverline, colors.gui_tab_selected_overline);
        set(ImGuiCol.TabDimmed, colors.gui_tab_dimmed);
        set(ImGuiCol.TabDimmedSelected, colors.gui_tab_dimmed_selected);
        set(ImGuiCol.TabDimmedSelectedOverline, colors.gui_tab_dimmed_selected_overline);
        set(ImGuiCol.DockingPreview, colors.gui_docking_preview);
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
        set(ImGuiCol.DragDropTarget, colors.gui_drag_drop_target);
        //NavCursor,
        //NavWindowingHighlight,
        //NavWindowingDimBg,
        //ModalWindowDimBg,

        if (style?.window_padding != null) set(ImGuiStyleVar.WindowPadding, style.Value.window_padding.Value);
        if (style?.cell_padding != null) set(ImGuiStyleVar.CellPadding, style.Value.cell_padding.Value);
        if (style?.frame_padding != null) set(ImGuiStyleVar.FramePadding, style.Value.frame_padding.Value);
        if (style?.separator_text_padding != null) set(ImGuiStyleVar.SeparatorTextPadding, style.Value.separator_text_padding.Value);
        
        pushes++;
    }
    
    private static void set(ImGuiCol id, color color) {
        
        ImGui.PushStyleColor(id, color.to_vector4());
        pushed_colors[pushes]++;
    }
    
    private static void set(ImGuiStyleVar id, Vector2 value) {
        
        ImGui.PushStyleVar(id, value);
        pushed_vars[pushes]++;
    }

    private static unsafe void debug(ImGuiCol id) {

        Console.WriteLine(ImGui.GetStyleColorVec4(id)->AsVector3());
    }

    public static void pop() {

        for (var i = 0; i < pushed_colors[pushes - 1]; i++) ImGui.PopStyleColor();
        pushed_colors.RemoveAt(pushes - 1);
        
        for (var i = 0; i < pushed_vars[pushes - 1]; i++) ImGui.PopStyleVar();
        pushed_vars.RemoveAt(pushes - 1);

        pushes--;
    }
}

public struct custom_style {

    public Vector2? window_padding = null;
    public Vector2? cell_padding = null;
    public Vector2? frame_padding = null;
    public Vector2? separator_text_padding = null;
    
    public custom_style() {}
}