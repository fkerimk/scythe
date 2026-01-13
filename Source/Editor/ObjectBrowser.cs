using System.Reflection;
using ImGuiNET;

internal class ObjectBrowser() : Viewport("Object") {
    
    public Obj? obj;

    private int prop_index;

    protected override void OnDraw() {
        
        if (obj == null) return;

        if (obj.Parent != null) {
            
            ImGui.PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.to_vector4());
            ImGui.Text(obj.Parent.Name);
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        
        ImGui.Text(obj.Name);
        
        ImGui.Spacing();

        ImGui.Separator();
        
        prop_index = 0;
        
        var type = obj.GetType();

        foreach (var prop in type.GetProperties()) 
            draw_property(obj, prop);

        if (obj.Type != null) {
            
            var clas_type = obj.Type.GetType();
            
            foreach (var prop in clas_type.GetProperties()) 
                draw_property(obj.Type, prop);
        }
    }

    public void draw_property(object target, PropertyInfo prop) {

        var id = $"##prop{prop_index}";
        
        var label_attr = prop.GetCustomAttribute<Label>();

        if (label_attr == null) return;
        
        ImGui.Text(label_attr.Value);

        var value = prop.GetValue(target);
            
        if (value != null) {
                
            ImGui.SameLine();

            if (prop.PropertyType == typeof(string)) {

                var cast_value = (string)value;
                ImGui.PushItemWidth(-1);
                ImGui.InputTextWithHint(id, "object", ref cast_value, 512);
                ImGui.PopItemWidth();
                prop.SetValue(target, cast_value);
            }
            
            if (prop.PropertyType == typeof(float3)) {

                var cast_value = (float3)value;
                var converted_value = cast_value.to_vector3();
                ImGui.PushItemWidth(-1);
                ImGui.InputFloat3(id, ref converted_value);
                ImGui.PopItemWidth();
                prop.SetValue(target, converted_value.to_float3());
            }
            
            if (prop.PropertyType == typeof(Color)) {

                var cast_value = (Color)value;
                var converted_value = cast_value.to_vector4();
                ImGui.PushItemWidth(-1);
                ImGui.InputFloat4(id, ref converted_value);
                ImGui.PopItemWidth();
                prop.SetValue(target, converted_value.to_color());
            }
            
            if (prop.PropertyType == typeof(int)) {

                var cast_value = (int)value;
                ImGui.PushItemWidth(-1);
                ImGui.InputInt(id, ref cast_value);
                ImGui.PopItemWidth();
                prop.SetValue(target, cast_value);
            }
            
            if (prop.PropertyType == typeof(bool)) {

                var cast_value = (bool)value;
                ImGui.PushItemWidth(-1);
                ImGui.Checkbox(id, ref cast_value);
                ImGui.PopItemWidth();
                prop.SetValue(target, cast_value);
            }
            
            if (prop.PropertyType == typeof(float)) {

                var cast_value = (float)value;
                ImGui.PushItemWidth(-1);
                ImGui.InputFloat(id, ref cast_value);
                ImGui.PopItemWidth();
                prop.SetValue(target, cast_value);
            }
        }
        
        prop_index++;
    }
}