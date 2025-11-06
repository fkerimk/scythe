using System.Numerics;

namespace scythe;

#pragma warning disable CS8981
public class obj {
    
    public string name;
    public string type;
    public obj? parent;
    public List<obj> children = [];
    //public prs prs = new prs(0, 0, 0, 0, 0, 0, 1, 1, 1);
    public Matrix4x4 matrix = Matrix4x4.Identity;
    public type? type_class = null;
    
    public obj(string name, string type, obj? parent = null) {

        this.name = name;
        this.type = type;
        this.parent = parent;
        
        type_class = type switch {
        
            "model" => new model(this),
            "transform" => new transform(this),
            "animation" => new animation(this),
        
            _ => null
        };
    }
    
    public void delete() {

        if (parent == null) return;
        
        parent.children.Remove(this);
        parent.order_children();
    }
    
    public void set_parent(obj obj) {
        
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