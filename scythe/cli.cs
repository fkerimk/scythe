namespace scythe;

#pragma warning disable CS8981
public static class cli {
    
    private static Dictionary<string, string>? _arguments;

    public static void init() {
        
        if (_arguments != null) return;

        var args = Environment.GetCommandLineArgs();
        _arguments = parse(args);
    }

    private static Dictionary<string, string> parse(string[] args) {
        
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i < args.Length; i++) {
            
            var currentArg = args[i];

            if ((currentArg.StartsWith("-") || currentArg.StartsWith("+")) && i + 1 < args.Length) {
                
                var key = currentArg[1..];
                var value = args[i + 1];

                if (!(value.StartsWith("-") || value.StartsWith("+"))) {
                    
                    dict[key] = value;
                    i++;

                    continue;
                }
            }

            if (!currentArg.StartsWith("-") && !currentArg.StartsWith("+")) continue; {
                
                var key = currentArg[1..];
                dict[key] = "true";
            }
        }

        return dict;
    }

    public static bool get(string argument, out string? value) {
        
        if (_arguments != null && _arguments.TryGetValue(argument, out value))
            return true;
        
        value = null;
        return false;
    }
}