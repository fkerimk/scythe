using System.Reflection;
using ImGuiNET;

internal class ObjectBrowser() : Viewport("Object") {
    
    public Obj? Obj;

    private int _propIndex;

    protected override void OnDraw() {
        
        if (Obj == null) return;

        if (Obj.Parent != null) {
            
            ImGui.PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.to_vector4());
            ImGui.Text(Obj.Parent.Name);
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        
        ImGui.Text(Obj.Name);
        
        ImGui.Spacing();

        ImGui.Separator();
        
        _propIndex = 0;
        
        var type = Obj.GetType();

        foreach (var prop in type.GetProperties()) 
            DrawProperty(Obj, prop);

        if (Obj.Type != null) {
            
            var clasType = Obj.Type.GetType();
            
            foreach (var prop in clasType.GetProperties()) 
                DrawProperty(Obj.Type, prop);
        }
    }

    public void DrawProperty(object target, PropertyInfo prop) {

        var id = $"##prop{_propIndex}";
        
        var labelAttr = prop.GetCustomAttribute<Label>();

        if (labelAttr == null) return;
        
        ImGui.Text(labelAttr.Value);

        var value = prop.GetValue(target);
            
        if (value != null) {
                
            ImGui.SameLine();

            if (prop.PropertyType == typeof(string)) {

                var castValue = (string)value;
                ImGui.PushItemWidth(-1);
                ImGui.InputTextWithHint(id, "object", ref castValue, 512);
                ImGui.PopItemWidth();
                prop.SetValue(target, castValue);
            }
            
            if (prop.PropertyType == typeof(float3)) {

                var castValue = (float3)value;
                var convertedValue = castValue.to_vector3();
                ImGui.PushItemWidth(-1);
                ImGui.InputFloat3(id, ref convertedValue);
                ImGui.PopItemWidth();
                prop.SetValue(target, convertedValue.to_float3());
            }
            
            if (prop.PropertyType == typeof(Color)) {

                var castValue = (Color)value;
                var convertedValue = castValue.to_vector4();
                ImGui.PushItemWidth(-1);
                ImGui.InputFloat4(id, ref convertedValue);
                ImGui.PopItemWidth();
                prop.SetValue(target, convertedValue.to_color());
            }
            
            if (prop.PropertyType == typeof(int)) {

                var castValue = (int)value;
                ImGui.PushItemWidth(-1);
                ImGui.InputInt(id, ref castValue);
                ImGui.PopItemWidth();
                prop.SetValue(target, castValue);
            }
            
            if (prop.PropertyType == typeof(bool)) {

                var castValue = (bool)value;
                ImGui.PushItemWidth(-1);
                ImGui.Checkbox(id, ref castValue);
                ImGui.PopItemWidth();
                prop.SetValue(target, castValue);
            }
            
            if (prop.PropertyType == typeof(float)) {

                var castValue = (float)value;
                ImGui.PushItemWidth(-1);
                ImGui.InputFloat(id, ref castValue);
                ImGui.PopItemWidth();
                prop.SetValue(target, castValue);
            }
        }
        
        _propIndex++;
    }
}