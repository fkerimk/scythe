using System.Numerics;
using System.Reflection;

namespace scythe;

public class obj {

    public string icon => type?.label_icon ?? icons.obj;
    public color color => type?.label_color ?? colors.gui_type_object;
    
    [label("Name")] public string name { get; set; }

    public obj? parent;
    public readonly List<obj> children = [];
    public Matrix4x4 matrix = Matrix4x4.Identity;
    public readonly type? type;
    public bool is_selected;
    
    public obj(string name, Type? type, obj? parent = null) {
        
        this.name = name;
        this.parent = parent;

        if (type == null || type == typeof(obj)) this.type = null;
        else this.type = (type?)(Activator.CreateInstance(type, this) ?? Activator.CreateInstance(type));
    }
    
    public void delete() {

        if (parent == null) return;
        
        parent.children.Remove(this);
        parent.order_children();
    }
    
    public void set_parent(obj? obj) {
        
        if (obj == null) return;
        if (obj == this) return;
        if (parent == null) return;
        
        parent.children.Remove(this);
        obj.children.Add(this);
        parent = obj;

        parent.order_children();
    }

    private static readonly System.Text.RegularExpressions.Regex natural_regex = new(@"(\d+)");

    private void order_children() {
    
        children.Sort((a, b) => natural_compare(a.name, b.name));
    }

    private static int natural_compare(string a, string b) {
    
        var tokens_a = natural_regex.Split(a);
        var tokens_b = natural_regex.Split(b);

        for (var i = 0; i < Math.Min(tokens_a.Length, tokens_b.Length); i++) {
        
            if (int.TryParse(tokens_a[i], out var aNum) && int.TryParse(tokens_b[i], out var bNum)) {
            
                var cmp = aNum.CompareTo(bNum);
                if (cmp != 0) return cmp;
            
            } else {
            
                var cmp = string.Compare(tokens_a[i], tokens_b[i], StringComparison.Ordinal);
                if (cmp != 0) return cmp;
            }
        }

        return tokens_a.Length.CompareTo(tokens_b.Length);
    }
}