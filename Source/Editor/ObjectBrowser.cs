using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Raylib_cs;
using static ImGuiNET.ImGui;

internal class ObjectBrowser : Viewport {
    
    private int _propIndex;
    private readonly IEnumerable<Type> _addComponentTypes;
    private string[] _foundFiles = [];
    private string _searchFilter = "";

    public ObjectBrowser() : base("Object") {
        var hideComponents = new[] { "Transform" };
        _addComponentTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract && !hideComponents.Contains(t.Name));
    }
    
    protected override void OnDraw() {
        
        _propIndex = 0;

        // 1. ASSET INSPECTION
        if (LevelBrowser.SelectedObjects.Count == 0) {
            
            var selectedFile = Editor.ProjectBrowser.SelectedFile;
            
            if (!string.IsNullOrEmpty(selectedFile))
                DrawAssetInspector(selectedFile.Replace('\\', '/'));
            
            return;
        }
        
        if (Core.ActiveLevel == null) return;
        var targets = LevelBrowser.SelectedObjects;

        // Header info
        PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.ToVector4());
        
        if (targets.Count == 1) {
            
            if (targets[0].Parent != null) {
                
                Text(targets[0].Parent?.Name);
                SameLine();
            }
        }
        
        else Text($"{targets.Count} objects selected");
        
        PopStyleColor();

        Separator(); Spacing();

        // 2. OBJECT & COMPONENT INSPECTION
        DrawProperties(targets.Cast<object>().ToList(), false, "Object");
        DrawProperties(targets.Select(t => (object)t.Transform).ToList(), true, "Transform");

        var firstObj = targets[0];
        
        var commonCompNames = firstObj.Components.Keys
            .Where(k => targets.All(t => t.Components.ContainsKey(k)))
            .OrderBy(k => k, new NaturalStringComparer());

        foreach (var compName in commonCompNames) {
            
            var compInstances = targets.Select(t => (object)t.Components[compName]).ToList();
            DrawProperties(compInstances, true, compName);
        }

        DrawAddComponentButton(targets);
    }

    private void DrawAddComponentButton(List<Obj> targets) {
        
        if (targets.Count != 1) return;
        
        Spacing(); Separator(); Spacing();
        
        if (Button("Add Component", new Vector2(GetContentRegionAvail().X, 0))) 
            OpenPopup("AddComponentPopup");

        if (BeginPopup("AddComponentPopup")) {
            
            foreach (var type in _addComponentTypes) {
                
                if (!Selectable(type.Name)) continue;
                var targetObj = targets[0];
                if (targetObj.Components.ContainsKey(type.Name)) continue;

                if (Activator.CreateInstance(type, targetObj) is not Component component) continue;
                var compName = type.Name;
                
                History.StartRecording(targetObj, $"Add Component {compName}");
                targetObj.Components[compName] = component;
                if (Core.ActiveLevel != null) Core.ActiveLevel.IsDirty = true;
                
                History.StopRecording();
                if (component is Animation anim && targetObj.Components.TryGetValue("Model", out var m))
                    anim.Path = (m as Model)!.Path;
            }
            
            EndPopup();
        }
    }

    // --- SHARED INSPECTOR CORE ---

    private static void DrawShadowedLabel(string label) {
        
        AlignTextToFramePadding();
        PushFont(Fonts.ImMontserratRegular);
        var cp = GetCursorPos();
        var cleanLabel = Generators.SplitCamelCase(label);
        Text(cleanLabel);
        SetCursorPos(cp + new Vector2(0.3f, 0));
        Text(cleanLabel);
        PopFont();
        NextColumn();
    }

    private bool DrawInspectorField(string id, ref object? value, Type type, List<object> targets, string? propName, string? pickerType = null) {
        
        var changed = false;
        
        PushItemWidth(-1); // Fill the entire column

        // Asset Picker Logic
        if (!string.IsNullOrEmpty(pickerType)) {
            
            PushFont(Fonts.ImFontAwesomeSmall);
            
            if (Button($"{Icons.FaSearch}##{id}_btn")) {
                
                List<(string Name, string Path)> names = pickerType switch {
                    
                    "ShaderAsset" => AssetManager.GetNames<ShaderAsset>(),
                    "TextureAsset" => AssetManager.GetNames<TextureAsset>(),
                    "ModelAsset" => AssetManager.GetNames<ModelAsset>(),
                    "AnimationAsset" => AssetManager.GetNames<AnimationAsset>(),
                    "MaterialAsset" => AssetManager.GetNames<MaterialAsset>(),
                    "ScriptAsset" => AssetManager.GetNames<ScriptAsset>(),
                    _ => new List<(string, string)>()
                };
                
                if (names.Count == 0) {
                    
                     var baseDir = PathUtil.ModRelative(pickerType);
                     
                     if (Directory.Exists(baseDir)) {
                         
                         var files = Directory.GetFiles(baseDir, "*.*", SearchOption.AllDirectories);
                         names = files.Select(f => (Path.GetFileNameWithoutExtension(f), f.Replace('\\', '/'))).ToList();
                     }
                }
                
                _foundFiles = names.Select(n => n.Path).ToArray();
                _searchFilter = "";
                
                OpenPopup($"Picker_{id}");
            }
            
            if (IsItemActivated() && propName != null) targets.ForEach(t => History.StartRecording(t, propName));
            
            SameLine();
            if (Button($"{Icons.FaXMark}##{id}_clear")) { value = ""; changed = true; }
            if (IsItemActivated() && propName != null) targets.ForEach(t => History.StartRecording(t, propName));
            PopFont(); SameLine();
            
            SetNextItemWidth(GetContentRegionAvail().X);
        }

        // Field drawing
        if (type == typeof(string)) {
            var val = (string)(value ?? "");
            var display = Path.GetFileNameWithoutExtension(val);
            if (string.IsNullOrEmpty(display)) display = val;
            if (InputTextWithHint($"##{id}", "None", ref display, 512, string.IsNullOrEmpty(pickerType) ? ImGuiInputTextFlags.None : ImGuiInputTextFlags.ReadOnly)) {
                if (string.IsNullOrEmpty(pickerType)) { value = display; changed = true; }
            }
        }
        else if (type == typeof(float)) {
            var val = (float)(value ?? 0f);
            if (InputFloat($"##{id}", ref val)) { value = val; changed = true; }
        }
        else if (type == typeof(int)) {
            var val = (int)(value ?? 0);
            if (InputInt($"##{id}", ref val)) { value = val; changed = true; }
        }
        else if (type == typeof(bool)) {
            var val = (bool)(value ?? false);
            if (Checkbox($"##{id}", ref val)) { value = val; changed = true; }
        }
        else if (type == typeof(Vector3)) {
            var val = (Vector3)(value ?? Vector3.Zero);
            if (InputFloat3($"##{id}", ref val)) { value = val; changed = true; }
        }
        else if (type == typeof(Vector2)) {
            var val = (Vector2)(value ?? Vector2.Zero);
            if (InputFloat2($"##{id}", ref val)) { value = val; changed = true; }
        }
        else if (type == typeof(Color)) {
            var col = (Color)(value ?? Color.White);
            var v4 = col.ToVector4();
            if (ColorEdit4($"##{id}", ref v4, ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.NoInputs)) { value = v4.ToColor(); changed = true; }
        }

        // --- History Logic inside Universal Control ---
        if (IsItemActivated() && propName != null) 
            targets.ForEach(t => History.StartRecording(t, propName));
        
        if (IsItemDeactivatedAfterEdit()) History.StopRecording();

        if (IsItemHovered() && type == typeof(string) && !string.IsNullOrEmpty((string)value!)) 
            SetTooltip((string)value);

        // Picker Popup logic
        if (BeginPopup($"Picker_{id}")) {
            SetNextItemWidth(300);
            InputTextWithHint("##filter", "Search...", ref _searchFilter, 128);
            BeginChild("##files", new Vector2(400, 400));
            var nms = _foundFiles.Select(Path.GetFileNameWithoutExtension).ToList();
            for (var i = 0; i < _foundFiles.Length; i++) {
                var f = _foundFiles[i]; var n = nms[i];
                if (!string.IsNullOrEmpty(_searchFilter) && !f.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase)) continue;
                if (Selectable($"{n}##{f}")) { 
                    if (targets != null && propName != null) targets.ForEach(t => History.StartRecording(t, propName));
                    value = f; changed = true; CloseCurrentPopup(); 
                    History.StopRecording();
                }
                if (nms.Count(x => x == n) > 1) { SameLine(); TextDisabled(Path.GetRelativePath(Config.Mod.Path, f)); }
            }
            EndChild(); EndPopup();
        }

        PopItemWidth();
        NextColumn();
        return changed;
    }

    private void DrawSectionHeader(string title, string icon, Color color, out bool open, bool showRemove = false, Action? onRemove = null) {
        Spacing();
        var headerPos = GetCursorScreenPos();
        var headerSize = new Vector2(GetContentRegionAvail().X, GetFrameHeight());
        GetWindowDrawList().AddRectFilled(headerPos, headerPos + headerSize, GetColorU32(ImGuiCol.Header, 0.45f), 2.0f);
        
        PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 3));
        PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
        var flags = ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth;
        open = TreeNodeEx($"##{title}", flags);
        PopStyleColor(); PopStyleVar();

        SameLine(); SetCursorPosX(GetCursorPosX() - 7.5f); SetCursorPosY(GetCursorPosY() + 2.5f);
        PushFont(Fonts.ImFontAwesomeSmall);
        TextColored(color.ToVector4(), icon);
        PopFont();
        SameLine();
        PushFont(Fonts.ImMontserratRegular);
        Text(title);
        PopFont();

        if (showRemove && onRemove != null) {
            SameLine();
            var removeBtnX = GetContentRegionAvail().X + GetCursorPosX() - 22; 
            SetCursorPosX(removeBtnX);
            if (SmallButton($"X##rem_{title}")) onRemove();
        }

        if (open) {
            Spacing();
            PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8));
            Columns(2, $"##{title}_cols", false);
            SetColumnWidth(0, GetWindowWidth() * 0.3f); // Reduced label width
        }
    }

    private void EndSection(bool open) {
        if (open) {
            Columns(1);
            PopStyleVar();
            TreePop();
            Spacing();
        }
    }

    // --- ASSET INSPECTORS ---

    private void DrawAssetInspector(string path) {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (path.EndsWith(".material.json", StringComparison.OrdinalIgnoreCase)) {
            var asset = AssetManager.Get<MaterialAsset>(path);
            if (asset != null) DrawMaterialAssetInspector(asset);
        }
        else if (ext == ".fbx" || ext == ".obj" || ext == ".gltf") {
            var asset = AssetManager.Get<ModelAsset>(path);
            if (asset != null) DrawModelAssetInspector(asset);
        }
    }

    private void DrawModelAssetInspector(ModelAsset model) {
        PushID(model.GetHashCode());
        DrawSectionHeader("Model Asset", Icons.FaCube, Colors.GuiTypeModel, out var open);
        if (open) {
            for (var i = 0; i < model.Materials.Length; i++) {
                var name = (i < model.Meshes.Count && !string.IsNullOrEmpty(model.Meshes[i].Name)) ? model.Meshes[i].Name : $"Mesh {i}";
                DrawShadowedLabel(name);
                object? val = model.MaterialPaths[i];
                if (DrawInspectorField($"MeshMat_{i}", ref val, typeof(string), [], null, "MaterialAsset"))
                    model.ApplyMaterial(i, (string)val!);
            }
        }
        EndSection(open); PopID();
    }

    private void DrawMaterialAssetInspector(MaterialAsset mat) {
        PushID(mat.GetHashCode());
        DrawSectionHeader("Material Asset", Icons.FaFileImage, Colors.GuiTypeModel, out var open);
        if (open) {
            DrawShadowedLabel("Shader");
            object? shader = mat.Data.Shader;
            if (DrawInspectorField("Shader", ref shader, typeof(string), [], null, "ShaderAsset")) {
                mat.Data.Shader = (string)shader!; mat.Save(); mat.ApplyChanges();
            }

            var sa = AssetManager.Get<ShaderAsset>(mat.Data.Shader);
            if (sa != null) {
                foreach (var prop in sa.Properties) {
                    PushID(prop.Name);
                    DrawShadowedLabel(prop.Name);
                    object? val = null; var t = typeof(float); string? picker = null;

                    if (prop.Type == "sampler2D") { val = mat.Data.Textures.GetValueOrDefault(prop.Name, ""); t = typeof(string); picker = "TextureAsset"; }
                    else if (prop.Type == "float") { val = mat.Data.Floats.GetValueOrDefault(prop.Name, 0f); t = typeof(float); }
                    else if (prop.Type == "vec2") { val = mat.Data.Vectors.GetValueOrDefault(prop.Name, Vector2.Zero); t = typeof(Vector2); }
                    else if (prop.Type == "vec3" || prop.Type == "vec4") {
                        if (prop.Name.Contains("color", StringComparison.OrdinalIgnoreCase) || prop.Name.Contains("albedo", StringComparison.OrdinalIgnoreCase) || prop.Name.Contains("emiss", StringComparison.OrdinalIgnoreCase)) {
                            val = mat.Data.Colors.GetValueOrDefault(prop.Name, Color.White); t = typeof(Color);
                        } else {
                            val = prop.Type == "vec3" ? Vector3.Zero : Vector4.One; t = prop.Type == "vec3" ? typeof(Vector3) : typeof(Vector4);
                        }
                    }

                    if (val != null && DrawInspectorField(prop.Name, ref val, t, [], null, picker)) {
                        if (t == typeof(string)) mat.Data.Textures[prop.Name] = (string)val!;
                        else if (t == typeof(float)) mat.Data.Floats[prop.Name] = (float)val!;
                        else if (t == typeof(Vector2)) mat.Data.Vectors[prop.Name] = (Vector2)val!;
                        else if (t == typeof(Color)) mat.Data.Colors[prop.Name] = (Color)val!;
                        mat.Save(); mat.ApplyChanges();
                    }
                    PopID();
                }
            }
        }
        EndSection(open); PopID();
    }

    private void DrawProperties(List<object> targets, bool separator, string title) {
        var first = targets[0];
        PushID(first.GetHashCode());

        var open = true;
        if (separator) {
            var icon = (first is Component c) ? c.LabelIcon : Icons.FaCube;
            var color = (first is Component cc) ? cc.LabelColor : Colors.GuiTypeModel;
            var isRemovable = (first is Component and not Transform) && targets.Count == 1;
            
            DrawSectionHeader(title, icon, color, out open, isRemovable, () => {
                var comp = (first as Component)!; var targetObj = comp.Obj; var name = comp.GetType().Name;
                History.StartRecording(targetObj, $"Remove {name}");
                comp.UnloadAndQuit(); targetObj.Components.Remove(name);
                if (Core.ActiveLevel != null) Core.ActiveLevel.IsDirty = true;
                History.StopRecording();
            });
        } else {
            PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8));
            Columns(2, "##props", false);
            SetColumnWidth(0, GetWindowWidth() * 0.3f); // Reduced label width
        }

        if (open) {
            foreach (var prop in first.GetType().GetProperties()) {
                var labelAttr = prop.GetCustomAttribute<LabelAttribute>();
                if (labelAttr == null) continue;

                var id = $"##prop_{_propIndex++}";
                var values = targets.Select(prop.GetValue).ToList();
                var allSame = values.All(v => Equals(v, values[0]));
                var val = allSame ? values[0] : null;

                DrawShadowedLabel(labelAttr.Value);
                
                var fileAttr = prop.GetCustomAttribute<FilePathAttribute>();
                var assetAttr = prop.GetCustomAttribute<FindAssetAttribute>();
                var picker = assetAttr?.TypeName ?? fileAttr?.Category;

                if (DrawInspectorField(id, ref val, prop.PropertyType, targets, prop.Name, picker)) {
                    foreach (var t in targets) {
                        prop.SetValue(t, val);
                        if (t is Component comp && (prop.Name == "Path" || fileAttr != null || assetAttr != null)) comp.UnloadAndQuit();
                    }
                    if (Core.ActiveLevel != null) Core.ActiveLevel.IsDirty = true;
                }
            }
        }

        if (separator) EndSection(open);
        else { Columns(1); PopStyleVar(); }
        PopID();
    }
}