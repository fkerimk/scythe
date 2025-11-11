using System.Reflection;
using ImGuiNET;

namespace scythe;

#pragma warning disable CS8981
#pragma warning disable IL2075
public class object_browser() : viewport("Object", ImGuiWindowFlags.NoCollapse) {
    
    public obj? obj;

    private int prop_index;

    protected override void on_draw() {
        
        if (obj == null) return;

        if (obj.parent != null) {
            
            ImGui.PushStyleColor(ImGuiCol.Text, colors.gui_text_disabled.to_vector4());
            ImGui.Text(obj.parent.name);
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        
        ImGui.Text(obj.name);
        
        ImGui.Spacing();

        ImGui.Separator();
        
        prop_index = 0;
        
        var type = obj.GetType();

        foreach (var prop in type.GetProperties()) 
            draw_property(obj, prop);

        if (obj.type != null) {
            
            var clas_type = obj.type.GetType();
            
            foreach (var prop in clas_type.GetProperties()) 
                draw_property(obj.type, prop);
        }
    }

    public void draw_property(object target, PropertyInfo prop) {

        var id = $"##prop{prop_index}";
        
        var label_attr = prop.GetCustomAttribute<label>();

        if (label_attr == null) return;
        
        ImGui.Text(label_attr.value);

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
            
            if (prop.PropertyType == typeof(color)) {

                var cast_value = (color)value;
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