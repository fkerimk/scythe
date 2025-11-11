using System.Text;

namespace scythe;
    
#pragma warning disable CS8981
public class ini : IDisposable {
    
    public bool is_valid;
    public readonly string path;
    public readonly Dictionary<string, Dictionary<string, string>> data;
    
    private readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;

    public ini(string path) {
        
        this.path = path;
        data = new(comparer);

        is_valid = false;

        if (!File.Exists(path)) return;
        
        Dictionary<string, string>? current = null;
        
        foreach (var raw in File.ReadLines(path)) {
            
            var line = raw.Trim();
            
            if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;

            if (line.StartsWith("[") && line.EndsWith("]")) {
                
                var sec = line[1..^1].Trim();
                
                if (!data.TryGetValue(sec, out current)) {
                    
                    current = new(comparer);
                    data[sec] = current;
                }
                
                continue;
            }

            var eq = line.IndexOf('=');
            if (eq <= 0) continue;

            var key = line[..eq].Trim();
            var val = line[(eq + 1)..].Trim();
            
            val = StripInlineComment(val);

            if (val.Length >= 2 && val.StartsWith('"') && val.EndsWith('"'))
                val = val[1..^1];

            val = val.Replace(@"\n", "\n").Replace(@"\t", "\t").Replace(@"\\", @"\");

            (current ??= GetOrCreateDefaultSection())[key] = val;
        }

        if (data.Count > 0) is_valid = true;
    }

    private Dictionary<string, string> GetOrCreateDefaultSection() {
        
        if (data.TryGetValue("", out var def)) return def;
        def = new(comparer);
        data[""] = def;
        return def;
    }

    private static string StripInlineComment(string value) {
        
        var inQuotes = false;
        
        for (var i = 0; i < value.Length; i++) {
            
            var c = value[i];
            
            if (c == '"' && (i == 0 || value[i - 1] != '\\'))
                inQuotes = !inQuotes;
            
            else if (!inQuotes && (c == ';' || c == '#'))
                return value[..i].TrimEnd();
        }
        
        return value;
    }

    public string read(string section, string key, string default_value = "") {
        
        return data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val)
            ? val
            : default_value;
    }
    
    public int read(string section, string key, int default_value = 0) {
        
        if (!data.TryGetValue(section, out var sec) ||
            !sec.TryGetValue(key, out var val) ||
            !int.TryParse(val, out var result))
            return default_value;
        
        return result;
    }
    
    public bool read(string section, string key, bool default_value = false) {
        
        if (!data.TryGetValue(section, out var sec) ||
            !sec.TryGetValue(key, out var val) ||
            !bool.TryParse(val, out var result))
            return default_value;
        
        return result;
    }

    public object read(string section, string key, object default_value) {
        
        return default_value switch {
            
            int int_value => read(section, key, int_value),
            bool bool_value => read(section, key, bool_value),
            string string_value => read(section, key, string_value),
            
            _ => default_value
        };
    }

    public void write(string section, string key, string value) {
        
        if (!data.TryGetValue(section, out var sec)) {
            
            sec = new(comparer);
            data[section] = sec;
        }
        
        sec[key] = value;
    }

    public void save() {
        
        var sb = new StringBuilder();
        
        foreach (var section in data) {
            
            if (section.Key != "")
                sb.AppendLine($"[{section.Key}]");

            foreach (var kvp in section.Value) {
                
                var val = kvp.Value.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\\", "\\\\");
                sb.AppendLine($"{kvp.Key}={val}");
            }
            
            sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    public void Dispose() {
        
        data.Clear();
    }
}