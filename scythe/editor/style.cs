using ImGuiNET;

namespace scythe;

public static class style {

    public static void push() {
        
        ImGui.PushStyleColor(ImGuiCol.Text, colors.gui_text.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, colors.gui_text_disabled.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.WindowBg, colors.gui_window_bg.to_vector4());
        //ChildBg,
        //PopupBg,
        ImGui.PushStyleColor(ImGuiCol.Border, colors.gui_border.to_vector4());
        //BorderShadow,
        //FrameBg,
        //FrameBgHovered,
        //FrameBgActive,
        ImGui.PushStyleColor(ImGuiCol.TitleBg, colors.gui_title_bg.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, colors.gui_title_bg_active.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, colors.gui_title_bg_collapsed.to_vector4());
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
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, colors.gui_resize_grip.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.ResizeGripHovered, colors.gui_resize_grip_hovered.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, colors.gui_resize_grip_active.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TabHovered, colors.gui_tab_hovered.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.Tab, colors.gui_tab.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TabSelected, colors.gui_tab_selected.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TabSelectedOverline, colors.gui_tab_selected_overline.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TabDimmed, colors.gui_tab_dimmed.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TabDimmedSelected, colors.gui_tab_dimmed_selected.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.TabDimmedSelectedOverline, colors.gui_tab_dimmed_selected_overline.to_vector4());
        ImGui.PushStyleColor(ImGuiCol.DockingPreview, colors.gui_docking_preview.to_vector4());
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
        // ImGui.PushStyleColor(ImGuiCol.Header, colors.gui_bg.to_vector4());
        // ImGui.PushStyleColor(ImGuiCol.HeaderHovered, colors.gui_bg.to_vector4());
        // ImGui.PushStyleColor(ImGuiCol.HeaderActive, colors.gui_bg.to_vector4());
        // ImGui.PushStyleColor(ImGuiCol.FrameBg, colors.gui_frame.to_vector4());
        // ImGui.PushStyleColor(ImGuiCol.FrameBgActive, colors.gui_frame.to_vector4());
        // ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, colors.gui_frame.to_vector4());
    }

    public static void pop() {

        for (var i = 0; i < 18; i++)
            ImGui.PopStyleColor();
    }
}