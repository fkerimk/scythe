using System.Reflection;
using System.Text;
using MoonSharp.Interpreter;

internal static class LuaDefinitionGenerator {
    
    private static readonly HashSet<string> ProcessedTypeNames = [];
    private static readonly HashSet<Type> ProcessedTypes = [];

    public static void Generate(MoonSharp.Interpreter.Script lua, string outputPath) {
        
        var sb = new StringBuilder();
        var queue = new Queue<Type>();
        
        ProcessedTypeNames.Clear();
        ProcessedTypes.Clear();

        sb.AppendLine("---@meta");
        sb.AppendLine();

        // Globals
        foreach (var global in lua.Globals.Pairs) {
            
            if (global.Value.Type != DataType.UserData) continue;
            var type = global.Value.UserData.Descriptor.Type;
            
            var typeName = CleanName(type.Name);
            sb.AppendLine($"---@type {typeName}");
            sb.AppendLine($"{global.Key.String} = nil");
            sb.AppendLine();
            
            EnqueueType(queue, type);
        }

        // Types
        while (queue.Count > 0) {
            
            var type = queue.Dequeue();
            
            if (type.IsByRef) type = type.GetElementType();

            if (type == null) continue;
            
            var className = CleanName(type.Name);

            if (ProcessedTypeNames.Contains(className) || string.IsNullOrEmpty(className)) continue;

            // Pass System.Numerics (Vector2, Vector3 etc.)
            if (type.Namespace != null) {
                
                if (type.Namespace.StartsWith("System") && !type.Namespace.StartsWith("System.Numerics")) continue;
                if (type.Namespace.StartsWith("MoonSharp")) continue;
                if (type.Namespace.StartsWith("Microsoft")) continue;
            }

            ProcessedTypeNames.Add(className);
            ProcessedTypes.Add(type);
            
            GenerateClass(sb, type, className, queue);
        }

        File.WriteAllText(outputPath, sb.ToString());
    }

    private static void EnqueueType(Queue<Type> queue, Type? t) {
        
        if (t == null) return;
        if (t.IsByRef) t = t.GetElementType();
        if (t != null && (t.IsPrimitive || t == typeof(string) || t == typeof(object) || t == typeof(void))) return;
        if (t == null) return;
        
        // Filter
        if (t.Namespace != null && t.Namespace.StartsWith("System") && !t.Namespace.StartsWith("System.Numerics")) return;
        if (t.Namespace != null && t.Namespace.StartsWith("MoonSharp")) return;

        if (!ProcessedTypes.Contains(t)) queue.Enqueue(t);
    }

    private static void GenerateClass(StringBuilder sb, Type type, string className, Queue<Type> queue) {

        var inheritance = "";
        
        if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType)) {

            var ns = type.BaseType.Namespace;
            
            var isForbiddenSystem = ns != null && ns.StartsWith("System") && !ns.StartsWith("System.Numerics");
            var isMoonSharp = ns != null && ns.StartsWith("MoonSharp");
            
            if (!isForbiddenSystem && !isMoonSharp) {
                
                 inheritance = $" : {CleanName(type.BaseType.Name)}";
                 EnqueueType(queue, type.BaseType);
            }
        }
        
        sb.AppendLine($"---@class {className}{inheritance}");

        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        
        var generatedMembers = new HashSet<string>();

        // Fields & properties (Vector2.X -> vector2.x etc..)
        foreach (var p in type.GetProperties(flags)) {
            
            if (Attribute.IsDefined(p, typeof(MoonSharpHiddenAttribute))) continue;
            
            var safeName = ToCamelCase(p.Name);
            if (!IsSafeName(p.Name) || generatedMembers.Contains(safeName)) continue;
            sb.AppendLine($"---@field {safeName} {MapType(p.PropertyType, queue)}");
            generatedMembers.Add(safeName);
        }
        
        foreach (var f in type.GetFields(flags)) {
            
            if (Attribute.IsDefined(f, typeof(MoonSharpHiddenAttribute))) continue;
            
            var safeName = ToCamelCase(f.Name);
            if (!IsSafeName(f.Name) || generatedMembers.Contains(safeName)) continue;
            sb.AppendLine($"---@field {safeName} {MapType(f.FieldType, queue)}");
            generatedMembers.Add(safeName);
        }

        sb.AppendLine($"local {className} = {{}}");

        // Methods
        foreach (var method in type.GetMethods(flags | BindingFlags.DeclaredOnly)) {
            
            if (Attribute.IsDefined(method, typeof(MoonSharpHiddenAttribute))) continue;
            
            if (method.IsSpecialName || !IsSafeName(method.Name)) continue;

            var safeMethodName = ToCamelCase(method.Name);
            
            if (!generatedMembers.Add(safeMethodName)) continue;

            var parameters = method.GetParameters();
            var paramNames = new List<string>();

            foreach (var p in parameters) {
                
                var pName = p.Name != null && IsSafeName(p.Name) ? p.Name : "arg" + p.Position;
                sb.AppendLine($"---@param {pName} {MapType(p.ParameterType, queue)}");
                paramNames.Add(pName);
            }

            // 'any' optimization
            var returnType = MapType(method.ReturnType, queue);
            
            if (method.Name.StartsWith("Find") || method.Name.StartsWith("Get") || returnType == "Obj" || returnType == "ObjType")
                returnType = "any"; 

            sb.AppendLine($"---@return {returnType}");
            
            var argString = string.Join(", ", paramNames);
            var sep = method.IsStatic ? "." : ":";

            sb.AppendLine($"function {className}{sep}{safeMethodName}({argString}) end");
            sb.AppendLine();
        }
    }

    private static string MapType(Type? t, Queue<Type> queue) {
        
        if (t == null) return "any";
        
        if (t.IsByRef) t = t.GetElementType();

        if (t == typeof(string)) return "string";
        if (t == typeof(bool)) return "boolean";
        
        if (t == null) return "any";
        
        if (t == typeof(void) || t.Name == "Void") return "void";
        if (t.IsPrimitive || t == typeof(decimal)) return "number";

        var name = CleanName(t.Name);
        if (string.IsNullOrEmpty(name) || t.IsGenericParameter) return "any";

        EnqueueType(queue, t);

        return name;
    }

    private static string ToCamelCase(string str) {
        
        if (string.IsNullOrEmpty(str)) return str;
        
        if (str.Length > 1 && char.IsUpper(str[0]))
            return char.ToLower(str[0]) + str.Substring(1);
        
        return str.ToLower();
    }

    private static string CleanName(string name) {
        
        if (string.IsNullOrEmpty(name)) return "";
        
        var tick = name.IndexOf('`');
        var clean = tick != -1 ? name[..tick] : name;
        
        clean = clean.Replace("&", "");
        
        return new string(clean.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    private static bool IsSafeName(string name) {
        
        if (string.IsNullOrEmpty(name)) return false;
        return char.IsLetter(name[0]) && name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}