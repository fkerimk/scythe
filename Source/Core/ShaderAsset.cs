using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

internal class ShaderAsset : Asset {
    
    public Shader Shader;
    
    private readonly Dictionary<string, int> _locations = new();

    public override unsafe bool Load() {
        
        var name = Path.GetFileNameWithoutExtension(File);
        string? vsPath = null;
        string? fsPath = null;

        // Try to find matching vs/fs in the same directory
        var directory = Path.GetDirectoryName(File)!;
        
        // Potential extensions
        if (System.IO.File.Exists(Path.Combine(directory, name + ".vs"))) 
            vsPath = Path.Combine(directory, name + ".vs");
            
        if (System.IO.File.Exists(Path.Combine(directory, name + ".fs"))) 
            fsPath = Path.Combine(directory, name + ".fs");

        if (vsPath == null && fsPath == null) return false;

        try {
            
            Shader = LoadShader(vsPath, fsPath);
            
            // Map standard locations
            var locAlbedo = GetShaderLocation(Shader, "albedo_map");
            if (locAlbedo != -1) Shader.Locs[(int)ShaderLocationIndex.MapAlbedo] = locAlbedo;
            
            var locNormal = GetShaderLocation(Shader, "normal_map");
            if (locNormal != -1) Shader.Locs[(int)ShaderLocationIndex.MapNormal] = locNormal;

            var locMetallic = GetShaderLocation(Shader, "metallic_map");
            if (locMetallic != -1) Shader.Locs[(int)ShaderLocationIndex.MapMetalness] = locMetallic;

            var locRoughness = GetShaderLocation(Shader, "roughness_map");
            if (locRoughness != -1) Shader.Locs[(int)ShaderLocationIndex.MapRoughness] = locRoughness;

            var locOcclusion = GetShaderLocation(Shader, "occlusion_map");
            if (locOcclusion != -1) Shader.Locs[(int)ShaderLocationIndex.MapOcclusion] = locOcclusion;
            
            var locEmission = GetShaderLocation(Shader, "emissive_map");
            if (locEmission != -1) Shader.Locs[(int)ShaderLocationIndex.MapEmission] = locEmission;
            
            // Map standard attributes
            Shader.Locs[(int)ShaderLocationIndex.VertexPosition] = GetShaderLocation(Shader, "vertex_pos");
            Shader.Locs[(int)ShaderLocationIndex.VertexTexcoord01] = GetShaderLocation(Shader, "vertex_tex_pos");
            Shader.Locs[(int)ShaderLocationIndex.VertexNormal] = GetShaderLocation(Shader, "vertex_normal");
            Shader.Locs[(int)ShaderLocationIndex.VertexTangent] = GetShaderLocation(Shader, "vertex_tangent");
            Shader.Locs[(int)ShaderLocationIndex.VertexColor] = GetShaderLocation(Shader, "vertex_color");

            // Standard Uniforms mapping (we handle albedo_color manually in Model.Draw to allow per-instance tinting)
            // var locColor = GetShaderLocation(Shader, "albedo_color");
            // if (locColor != -1) Shader.Locs[(int)ShaderLocationIndex.ColorDiffuse] = locColor;
            
            var locView = GetShaderLocation(Shader, "view_pos");
            if (locView != -1) Shader.Locs[(int)ShaderLocationIndex.VectorView] = locView;

            // Global defaults for tiling/offset
            var locTiling = GetShaderLocation(Shader, "tiling");
            if (locTiling != -1) SetShaderValue(Shader, locTiling, new Vector2(1.0f, 1.0f), ShaderUniformDataType.Vec2);
            var locOffset = GetShaderLocation(Shader, "offset");
            if (locOffset != -1) SetShaderValue(Shader, locOffset, new Vector2(0.0f, 0.0f), ShaderUniformDataType.Vec2);

            // Cubemap for Skybox
            var locEnv = GetShaderLocation(Shader, "environmentMap");
            if (locEnv != -1) SetShaderValue(Shader, locEnv, (int)MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);

            ParseUniforms(vsPath);
            ParseUniforms(fsPath);

        }
        
        catch { return false; }

        IsLoaded = true;
        return true;
    }

    public override void Unload() {
        
        if (IsLoaded) UnloadShader(Shader);
        
        IsLoaded = false;
        _locations.Clear();
    }
    
    public class ShaderProperty {
        
        public string Name = "";
        public string Type = "";
    }
    
    public List<ShaderProperty> Properties = [];

    private void ParseUniforms(string? path) {
        
        if (path == null || !System.IO.File.Exists(path)) return;
        
        var lines = System.IO.File.ReadAllLines(path);
        var regex = new System.Text.RegularExpressions.Regex(@"uniform\s+(float|int|vec2|vec3|vec4|sampler2D|samplerCube)\s+(\w+);");
        
        foreach (var line in lines) {
            
            var match = regex.Match(line);
            if (!match.Success) continue;
            
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
                
            // Skip standard uniforms
            if (name == "mvp" || name == "matModel" || name == "matNormal" || name == "view_pos" || 
                name == "light_count" || name.StartsWith("lights[") || name == "lightVP" ||
                name == "shadowMap" || name == "shadow_light_index" || name == "shadow_strength" ||
                name == "shadow_map_resolution" || name == "receive_shadows" ||
                // Skip Material Maps standard names if handled automatically, but maybe we want to expose them?
                // Let's expose everything not strictly system-internal.
                name == "tiling" || name == "alpha_cutoff") 
                continue;

            if (Properties.All(p => p.Name != name))
                Properties.Add(new ShaderProperty { Name = name, Type = type });
        }
    }

    public int GetLoc(string name) {
        
        if (_locations.TryGetValue(name, out var loc)) return loc;
        loc = GetShaderLocation(Shader, name);
        _locations[name] = loc;
        return loc;
    }
}
