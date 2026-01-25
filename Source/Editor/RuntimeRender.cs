using System.Numerics;
using ImGuiNET;
using Raylib_cs;

internal class RuntimeRender : Viewport {

    public RenderTexture2D Rt = new();
    public Vector2 TexSize = Vector2.One, TexTemp = Vector2.Zero;
    public Vector2 ScreenPos;

    public RuntimeRender() : base("Render (Runtime)") {
        
        CustomStyle = new CustomStyle {
            
            WindowPadding = Vector2.Zero
        };
    }

    protected override void OnDraw() {

        if (Rt.Texture.Id > 0) {
            
            var tex = (IntPtr)Rt.Texture.Id;
            var contentAvail = ImGui.GetContentRegionAvail();
            ScreenPos = ImGui.GetCursorScreenPos();
            
            // Draw game render
            ImGui.Image(tex, contentAvail, new Vector2(0, 1), new Vector2(1, 0));
            TexSize = contentAvail;

            // Draw overlay ui
            var padding = new Vector2(10, 10);
            ImGui.SetCursorScreenPos(ScreenPos + padding);
            
            // Semi-transparent background for the control panel
            var drawList = ImGui.GetWindowDrawList();
            var bgMin = ScreenPos + padding - new Vector2(4, 4);
            var bgMax = bgMin + new Vector2(145, 30); // Adjust based on content
            drawList.AddRectFilled(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.1f, 0.75f)), 6.0f);
            drawList.AddRect(bgMin, bgMax, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 0.1f)), 6.0f);

            ImGui.BeginGroup();
            
            var icon = Core.IsPlaying ? Icons.FaStop : Icons.FaPlay;
            var statusText = Core.IsPlaying ? "Playing..." : "Game View";
            var color = Core.IsPlaying ? Colors.Primary : Colors.GuiText;

            // Use FontAwesome for the button
            ImGui.PushFont(Fonts.ImFontAwesomeSmall);
            ImGui.PushStyleColor(ImGuiCol.Text, color.ToVector4());
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 4); 
            
            if (ImGui.Button($"{icon}##PlayToggle", new Vector2(30, 22))) {
                
                var center = ScreenPos + contentAvail / 2f;
                Editor.TogglePlayMode(center);
            }
            
            ImGui.PopStyleColor();
            ImGui.PopFont();

            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2); // Center text vertically
            ImGui.TextDisabled(statusText);
            
            ImGui.EndGroup();

            // Handle Focus & Mouse Locking
            if (ImGui.IsItemHovered() || ImGui.IsWindowHovered()) {
                
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseClicked(ImGuiMouseButton.Right)) {
                    
                    if (Core.IsPlaying && !LuaMouse.IsLocked) {
                        
                        LuaMouse.IsLocked = true;
                        
                        // Center mouse in viewport on click
                        var imgCenter = ScreenPos + contentAvail / 2f;
                        Raylib.SetMousePosition((int)imgCenter.X, (int)imgCenter.Y);
                    }
                }
            }

            const float fontSize = 26f;
            
            var fpsText = Window.GetFpsText();
            var textSize = ImGui.CalcTextSize(fpsText) * (fontSize / ImGui.GetFontSize());
            var fpsPadding = new Vector2(10, 10);
            
            var pos = ScreenPos + new Vector2(contentAvail.X - textSize.X - fpsPadding.X, fpsPadding.Y);
            
            drawList.AddText(ImGui.GetFont(), fontSize, pos + new Vector2(1, 1), ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)), fpsText);
            drawList.AddText(ImGui.GetFont(), fontSize, pos, ImGui.ColorConvertFloat4ToU32(Colors.Primary.ToVector4()), fpsText);
        }
        
        else ImGui.TextDisabled("Waiting for first frame...");
    }
}
