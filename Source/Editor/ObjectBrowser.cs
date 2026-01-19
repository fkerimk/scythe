using System.Numerics;
using System.Reflection;
using ImGuiNET;
using static ImGuiNET.ImGui;

internal class ObjectBrowser : Viewport {
    
    private int _propIndex;

    private readonly IEnumerable<Type> _addComponentTypes;
    
    public ObjectBrowser() : base("Object") {
        
        _addComponentTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => 
                t.IsSubclassOf(typeof(Component)) && !t.IsAbstract &&
                ! new [] { "Transform" }.Contains(t.Name)
            ) ;
    }
    
    protected override void OnDraw() {
        
        _propIndex = 0;

        if (LevelBrowser.SelectedObject == null) return;
        if (Core.ActiveLevel == null) return;

        if (LevelBrowser.SelectedObject.Parent != null) {

            PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.to_vector4());
            Text(LevelBrowser.SelectedObject.Parent.Name);
            PopStyleColor();
            SameLine();
        }

        Separator();
        Spacing();

        DrawProperties(LevelBrowser.SelectedObject, false, null);
        DrawProperties(LevelBrowser.SelectedObject.Transform, true, "Transform");

        foreach (var component in LevelBrowser.SelectedObject.Components.Values)
            DrawProperties(component, true, component.GetType().Name);

        Spacing();
        Separator();
        Spacing();

        if (Button("Add Component", new Vector2(GetContentRegionAvail().X, 0))) 
            OpenPopup("AddComponentPopup");

        SetNextWindowPos(new Vector2(GetItemRectMin().X, GetItemRectMax().Y + 5.0f));
        SetNextWindowSize(new Vector2(GetItemRectSize().X, 0));
        
        if (BeginPopup("AddComponentPopup")) {
            
            foreach (var type in _addComponentTypes) {
                    
                if (!Selectable(type.Name)) continue;

                if (LevelBrowser.SelectedObject.Components.ContainsKey(type.Name.ToPascalCase())) {

                    Notifications.Show($"Component {type.Name} already exists!", 1.5f);
                    break;
                }
                
                var component = Activator.CreateInstance(type, LevelBrowser.SelectedObject) as Component;

                if (component == null) continue;
                
                LevelBrowser.SelectedObject.Components[type.Name.ToPascalCase()] = component;

                // var builtObject = Core.ActiveLevel.RecordedBuildObject(type.Name, _obj.Parent, _obj.Components);

                //if (builtObject is { Components: Animation animation, Parent.Components: Model model })
                //    animation.Path = model.Path;

                // SelectObject(builtObject);
            }
            
            EndPopup();
        }
    }

    public void DrawProperties(object obj, bool seperator, string? title) {
        
        if (seperator) {
            
            Spacing();
            Separator();
            Spacing();
        }

        if (!string.IsNullOrEmpty(title)) {

            if (obj is Component iconComponent) {
                
                SameLine();
                PushFont(Fonts.ImFontAwesomeSmall);
                SetCursorPos(new Vector2(GetCursorPosX() - 7.5f, GetCursorPosY() + 2.5f));
                TextColored(iconComponent.LabelScytheColor.to_vector4(), iconComponent.LabelIcon);
                PopFont();
                SameLine();
            }

            Text(title);

            if (obj is Component component and not Transform) {
                
                var buttonSize = CalcTextSize("X").X + GetStyle().FramePadding.X * 2.0f;
                SameLine(GetContentRegionAvail().X - buttonSize);
            
                if (SmallButton($"X##{_propIndex}")) {

                    component.Obj.Components.Remove(component.Name.ToPascalCase());
                }
            }
            
            Spacing();
        }
        
        foreach (var prop in obj.GetType().GetProperties())
            DrawProperty(obj, prop);
    }
    
    private void DrawProperty(object target, PropertyInfo prop) {

        var id = $"##prop{_propIndex}";
        
        var labelAttr = prop.GetCustomAttribute<LabelAttribute>();

        if (labelAttr == null) return;
        
        Text(labelAttr.Value);

        var value = prop.GetValue(target);
            
        if (value != null) {
                
            SameLine();

            var recordedHistory = false;
            
            if (prop.PropertyType == typeof(string)) {

                var castValue = (string)value;
                PushItemWidth(-1);
                InputTextWithHint(id, "object", ref castValue, 512);
                PopItemWidth();

                if ((string)value != castValue) {
                    History.StartRecording(target, prop.Name);
                    recordedHistory = true;
                }
                
                prop.SetValue(target, castValue);
            }
            
            if (prop.PropertyType == typeof(Vector3)) {

                var castValue = (Vector3)value;
                var convertedValue = castValue;
                PushItemWidth(-1);
                InputFloat3(id, ref convertedValue);
                PopItemWidth();

                if (
                    MathF.Abs(castValue.X - convertedValue.X) > 0.001f ||
                    MathF.Abs(castValue.Y - convertedValue.Y) > 0.001f ||
                    MathF.Abs(castValue.Z - convertedValue.Z) > 0.001f
                ) {
                    History.StartRecording(target, prop.Name);
                    recordedHistory = true;
                }
                
                prop.SetValue(target, convertedValue);
            }
            
            if (prop.PropertyType == typeof(ScytheColor)) {

                var castValue = (ScytheColor)value;
                var convertedValue = castValue.to_vector4();
                PushItemWidth(-1);
                //ImGui.InputFloat4(id, ref convertedValue);
                ColorPicker4(id, ref convertedValue, ImGuiColorEditFlags.DisplayRGB);
                PopItemWidth();

                if (
                    MathF.Abs(castValue.R - convertedValue.X) > 0.001f ||
                    MathF.Abs(castValue.G - convertedValue.Y) > 0.001f ||
                    MathF.Abs(castValue.B - convertedValue.Z) > 0.001f ||
                    MathF.Abs(castValue.A - convertedValue.W) > 0.001f
                ) {
                    History.StartRecording(target, prop.Name);
                    recordedHistory = true;
                }
                
                prop.SetValue(target, convertedValue.ToColor());
            }
            
            if (prop.PropertyType == typeof(int)) {

                var castValue = (int)value;
                PushItemWidth(-1);
                InputInt(id, ref castValue);
                PopItemWidth();

                if ((int)value != castValue) {
                    History.StartRecording(target, prop.Name);
                    recordedHistory = true;
                }
                
                prop.SetValue(target, castValue);
            }
            
            if (prop.PropertyType == typeof(bool)) {

                var castValue = (bool)value;
                PushItemWidth(-1);
                Checkbox(id, ref castValue);
                PopItemWidth();

                if ((bool)value != castValue) {
                    History.StartRecording(target, prop.Name);
                    recordedHistory = true;
                }
                
                prop.SetValue(target, castValue);
            }
            
            if (prop.PropertyType == typeof(float)) {

                var castValue = (float)value;
                PushItemWidth(-1);
                InputFloat(id, ref castValue);
                PopItemWidth();

                if (MathF.Abs((float)value - castValue) > 0.001f) {
                    History.StartRecording(target, prop.Name);
                    recordedHistory = true;
                }
                
                prop.SetValue(target, castValue);
            }
            
            if (recordedHistory) History.StopRecording();
        }
        
        _propIndex++;
    }
}