using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace scythe;

#pragma warning disable CS8981
internal class editor() : raylib_session(1, 1, TraceLogLevel.Warning, ConfigFlags.Msaa4xHint, ConfigFlags.AlwaysRunWindow, ConfigFlags.ResizableWindow) {

    private ImGuiIOPtr io;
    
    private level_3d? level_3d;
    private level_browser? level_browser;
    private object_browser? object_browser;
    
    protected override unsafe void init() {
        
        resize_window(new(screen.width / 2, screen.height / 2));
        center_window();
        
        // imgui setup
        rlImGui.Setup(true, true);
        
        io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        //io.NativePtr->IniFilename = (byte*)IntPtr.Zero.ToPointer();
        io.NativePtr->IniFilename = (byte*)Marshal.StringToHGlobalAnsi("layout/user.ini").ToPointer();
        
        // generic initials
        core.init(true);
        freecam.init();
        fonts.load(io);

        if (cam.main == null) return;
        if (core.level == null) return;
        
        // viewports
        level_3d = new() { custom_style = new custom_style {
            window_padding = new(0, 0),
            cell_padding = new(0, 0),
            separator_text_padding = new(0, 0)
        }};
        
        level_browser = new(core.level);
        object_browser = new();
    }

    protected override void loop() {

        
        target_fps = config.editor.fps_lock;
        
        if (cam.main == null) return;
        if (level_3d == null) return;
        if (level_browser == null) return;
        if (object_browser == null) return;
        
        // reload viewport render
        if (level_3d.tex_size != level_3d.tex_temp) {
                
            Raylib.UnloadRenderTexture(level_3d.rt);
            level_3d.rt = Raylib.LoadRenderTexture((int)level_3d.tex_size.X, (int)level_3d.tex_size.Y);
            level_3d.tex_temp = level_3d.tex_size;
        }
            
        // render core on viewport
        Raylib.BeginTextureMode(level_3d.rt);
            
        clear(colors.game);

        // start camera
        freecam.loop(level_3d);
        
        cam.main.start_rendering();
            
        // 3d
        var grid = new grid(cam.main);
        grid.draw();
            
        core.loop_3d(true);
        core.loop_3d_editor(level_3d);
            
        // stop camera
        cam.main.stop_rendering();
            
        // ui
        core.loop_ui(true);
        core.loop_ui_editor(level_3d);
            
        if (config.editor.draw_fps) Raylib.DrawText($"{Raylib.GetFPS()}", 10, 10, 20, colors.primary.to_raylib());

        Raylib.EndTextureMode();
            
        // draw raylib
        Raylib.BeginDrawing();
        clear(new(0, 0, 0));
            
        // draw imgui
        rlImGui.Begin();
        style.push();
        ImGui.PushFont(fonts.montserrat_regular);
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport().ID);
        io.MouseDoubleClickTime = 0.2f;
            
        // draw elements
        level_3d.draw();
        level_browser.draw();
        object_browser.obj = level_browser.selected_object;
        object_browser.draw();
        
        ImGui.PopFont();
        style.pop();
        rlImGui.End();
    }

    protected override void quit() {
        
        core.quit();
    }
}