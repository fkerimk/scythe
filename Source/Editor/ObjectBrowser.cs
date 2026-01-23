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
    private List<object>? _pickerTargets;
    private PropertyInfo? _pickerProp;
    private bool _shouldOpenPicker;

    public ObjectBrowser() : base("Object") {

        var hideComponents = new[] {
            
            "Transform"
        };
        
        _addComponentTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Component)) && !t.IsAbstract && ! hideComponents.Contains(t.Name)) ;
    }
    
    protected override void OnDraw() {
        
        _propIndex = 0;

        if (LevelBrowser.SelectedObjects.Count == 0) return;
        if (Core.ActiveLevel == null) return;

        var targets = LevelBrowser.SelectedObjects;

        if (targets.Count == 1) {
            
            if (targets[0].Parent != null) {
                
                PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.ToVector4());
                Text(targets[0].Parent?.Name);
                PopStyleColor();
                SameLine();
            }
            
        } else {
            
             PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.ToVector4());
             Text($"{targets.Count} objects selected");
             PopStyleColor();
        }

        Separator();
        Spacing();

        // 1. Draw Obj properties (e.g. Name)
        DrawProperties(targets.Cast<object>().ToList(), false, null);

        // 2. Draw Transform properties
        var transforms = targets.Select(object (t) => t.Transform).ToList();
        DrawProperties(transforms, true, "Transform");

        // 3. Draw Common Components
        var firstObj = targets[0];
        
        var commonCompNames = firstObj.Components.Keys
            .Where(k => targets.All(t => t.Components.ContainsKey(k)))
            .OrderBy(k => k, new NaturalStringComparer());

        foreach (var compName in commonCompNames) {
            
            var compInstances = targets.Select(t => (object)t.Components[compName]).ToList();
            DrawProperties(compInstances, true, compName);
        }

        if (targets.Count == 1) {
            
            Spacing();
            Separator();
            Spacing();

            if (Button("Add Component", new Vector2(GetContentRegionAvail().X, 0))) 
                OpenPopup("AddComponentPopup");

            SetNextWindowPos(new Vector2(GetItemRectMin().X, GetItemRectMax().Y + 5.0f));
            SetNextWindowSize(new Vector2(GetItemRectSize().X, 0));

            if (BeginPopup("AddComponentPopup")) {
            
                foreach (var type in _addComponentTypes) {
                        
                    if (!Selectable(type.Name)) continue;

                    if (LevelBrowser.SelectedObject!.Components.ContainsKey(type.Name)) {

                        Notifications.Show($"Component {type.Name} already exists!", 1.5f);
                        break;
                    }

                    if (Activator.CreateInstance(type, LevelBrowser.SelectedObject) is not Component component) continue;
                    
                    // History
                    var targetObj = LevelBrowser.SelectedObject;
                    var compName = type.Name;
                    
                    History.StartRecording(targetObj, $"Add Component {compName}");
                    
                    targetObj.Components[compName] = component; // Do
                    
                    History.SetUndoAction(() => {
                        
                        if (!targetObj.Components.TryGetValue(compName, out var c)) return;
                        
                        c.UnloadAndQuit();
                        targetObj.Components.Remove(compName);
                    });
                    
                    History.SetRedoAction(() => {
                        
                        if (Activator.CreateInstance(type, targetObj) is not Component newComponent) return;
                        
                        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                            
                            if (!p.CanWrite || !p.CanRead || p.Name == "IsLoaded") continue;
                            SafeExec.Try(() => p.SetValue(newComponent, p.GetValue(component)));
                        }
                        
                        targetObj.Components[compName] = newComponent;
                    });
                    
                    History.StopRecording();

                    if (component is Animation animation &&  LevelBrowser.SelectedObject.Components.TryGetValue("Model", out var model))
                        animation.Path = (model as Model)!.Path;
                }
                
                EndPopup();
            }
        }

        DrawFilePicker();
    }

    private void DrawFilePicker() {
        
        if (_shouldOpenPicker) {
            
            OpenPopup("File Picker");
            _shouldOpenPicker = false;
        }

        if (!BeginPopup("File Picker")) return;
        
        SetNextItemWidth(300);
        InputTextWithHint("##filter", "Search...", ref _searchFilter, 128);
        
        BeginChild("##files", new Vector2(300, 400));
        
        foreach (var file in _foundFiles) {
            
            if (!string.IsNullOrEmpty(_searchFilter) && !file.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!Selectable(file)) continue;
            
            if (_pickerTargets != null && _pickerProp != null) {
                        
                foreach (var target in _pickerTargets) History.StartRecording(target, _pickerProp.Name);

                foreach (var target in _pickerTargets) {
                            
                    _pickerProp.SetValue(target, file);
                    if (target is Component comp) comp.UnloadAndQuit();
                }
                        
                History.StopRecording();
            }
            
            CloseCurrentPopup();
        }
        
        EndChild();

        EndPopup();
    }

    private void DrawProperties(List<object> targets, bool separator, string? title) {
        
        var first = targets[0];
        PushID(first.GetHashCode());
        
        if (separator && !string.IsNullOrEmpty(title)) {
            
            Spacing();
            
            var headerPos = GetCursorScreenPos();
            var headerSize = new Vector2(GetContentRegionAvail().X, GetFrameHeight());
            
            // Background
            GetWindowDrawList().AddRectFilled(headerPos, headerPos + headerSize, GetColorU32(ImGuiCol.Header, 0.45f), 2.0f);
            
            PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 3));
            PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
            
            var treeOpen = TreeNodeEx($"##{title}_header", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth);
            
            // Drag Source (Only for single selection)
            if (targets is [Component comp] && BeginDragDropSource()) {
                
                LevelBrowser.DragComponent = comp;
                SetDragDropPayload("component", IntPtr.Zero, 0);
                Text($"Reference {title}");
                EndDragDropSource();
            }

            PopStyleColor();
            PopStyleVar();

            // Content in header
            SameLine();
            SetCursorPosX(GetCursorPosX() - 7.5f); // Tighter arrow space
            SetCursorPosY(GetCursorPosY() + 3f);
            
            if (first is Component c) {

                PushFont(Fonts.ImFontAwesomeSmall);
                TextColored(c.LabelColor.ToVector4(), c.LabelIcon);
                PopFont();
                SameLine();
            }
            
            PushFont(Fonts.ImMontserratRegular);
            Text(title);
            PopFont();

            if (targets.Count == 1 && first is Component component and not Transform) {
                
                var buttonSize = CalcTextSize("X").X + GetStyle().FramePadding.X * 2.0f;
                var targetX = GetCursorPosX() + GetContentRegionAvail().X - buttonSize - 5;
                SameLine();
                SetCursorPosX(targetX);
            
                if (SmallButton($"X##{_propIndex}")) {
                    
                    var targetObj = component.Obj;
                    var compName = component.GetType().Name;
                    
                    History.StartRecording(targetObj, $"Remove Component {compName}");
                    
                    component.UnloadAndQuit();
                    targetObj.Components.Remove(compName); // Do
                    
                    History.SetUndoAction(() => {
                        
                        if (Activator.CreateInstance(component.GetType(), targetObj) is not Component newComponent) return;
                        
                        foreach (var p in component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                            
                            if (!p.CanWrite || !p.CanRead || p.Name == "IsLoaded") continue;
                            SafeExec.Try(() => p.SetValue(newComponent, p.GetValue(component)));
                        }
                            
                        targetObj.Components[compName] = newComponent;
                    });
                    
                    History.SetRedoAction(() => {
                        
                        if (!targetObj.Components.TryGetValue(compName, out var c)) return;
                        c.UnloadAndQuit();
                        targetObj.Components.Remove(compName);
                    });
                    
                    History.StopRecording();
                }
            }

            if (treeOpen) {
                Spacing();
            
                PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8));
                
                Columns(2, "##props", false);
                SetColumnWidth(0, GetWindowWidth() * 0.3f); 

                foreach (var prop in first.GetType().GetProperties()) {
                    
                    var headAttr = prop.GetCustomAttribute<HeaderAttribute>();
                    
                    if (headAttr != null) {
                        Columns(1);
                        Spacing();
                        PushFont(Fonts.ImMontserratRegular);
                        var headerCp = GetCursorPos();
                        TextColored(Colors.Primary.ToVector4(), headAttr.Title);
                        SetCursorPos(headerCp + new Vector2(0.3f, 0));
                        TextColored(Colors.Primary.ToVector4(), headAttr.Title);
                        PopFont();
                        Separator();
                        Spacing();
                        Columns(2, "##props", false);
                        SetColumnWidth(0, GetWindowWidth() * 0.3f);
                    }

                    var labelAttr = prop.GetCustomAttribute<LabelAttribute>();
                    if (labelAttr == null) continue;

                    AlignTextToFramePadding();
                    PushFont(Fonts.ImMontserratRegular);
                    var labelCp = GetCursorPos();
                    Text(labelAttr.Value);
                    SetCursorPos(labelCp + new Vector2(0.3f, 0));
                    Text(labelAttr.Value);
                    PopFont();
                    NextColumn();
                    
                    DrawProperty(targets, prop);
                    NextColumn();
                }
                    
                Columns(1);
                PopStyleVar();

                TreePop();
                Spacing();
            }
            
        } else {
             // Non-separated properties (e.g. main object)
             PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8));
             Columns(2, "##props", false);
             SetColumnWidth(0, GetWindowWidth() * 0.3f); 

             foreach (var prop in first.GetType().GetProperties()) {
                 
                var labelAttr = prop.GetCustomAttribute<LabelAttribute>();
                if (labelAttr == null) continue;

                AlignTextToFramePadding();
                PushFont(Fonts.ImMontserratRegular);
                var labelAltCp = GetCursorPos();
                Text(labelAttr.Value);
                SetCursorPos(labelAltCp + new Vector2(0.3f, 0));
                Text(labelAttr.Value);
                PopFont();
                NextColumn();
                DrawProperty(targets, prop);
                NextColumn();
             }
             
             Columns(1);
             PopStyleVar();
        }
        
        PopID();
    }

    private void DrawProperty(List<object> targets, PropertyInfo prop) {

        var id = $"##prop{_propIndex}";
        var values = targets.Select(prop.GetValue).ToList();
        var first = values[0];
        var allSame = values.All(v => Equals(v, first));
            
        var changed = false;
        object? newValue = null;
        
        PushItemWidth(-1);

        if (prop.PropertyType == typeof(string)) {

            var castValue = allSame ? (string)first! : "-";
            var filePathAttr = prop.GetCustomAttribute<FilePathAttribute>();

            if (filePathAttr != null) {
                
                PushFont(Fonts.ImFontAwesomeSmall);
                var pressed = Button($"{Icons.FaSearch}##{id}");
                PopFont();

                if (pressed) {
                    
                    var baseDir = PathUtil.ModRelative(filePathAttr.Category);
                    
                    if (Directory.Exists(baseDir)) {
                        
                        _foundFiles = Directory.GetFiles(baseDir, $"*{filePathAttr.Extension}", SearchOption.AllDirectories)
                            .Select(f => Path.GetRelativePath(baseDir, f).Replace('\\', '/'))
                            .Select(f => f.Substring(0, f.Length - filePathAttr.Extension.Length))
                            .ToArray();

                        if (_foundFiles.Length == 0)
                            Notifications.Show($"There is no {filePathAttr.Extension} file in {filePathAttr.Category}");
                        
                        else {
                            
                            _pickerTargets = targets; 
                            _pickerProp = prop;
                            _searchFilter = "";
                            _shouldOpenPicker = true;
                        }
                    }
                    
                    else Notifications.Show($"Folder not found: {filePathAttr.Category}");
                }
                
                SameLine();
            }

            if (InputTextWithHint(id, allSame ? "" : "Mixed", ref castValue, 512)) {
                
                newValue = castValue;
                changed = true;
            }
        }
        
        else if (prop.PropertyType == typeof(Vector3)) {

            var castValue = allSame ? (Vector3)first! : new Vector3(0);
            var result = castValue;
            
            if (InputFloat3(id, ref result)) {

                foreach (var t in targets) {
                    
                    var current = (Vector3)prop.GetValue(t)!;
                    
                    if (Math.Abs(result.X - castValue.X) > 0.001f) current.X = result.X;
                    if (Math.Abs(result.Y - castValue.Y) > 0.001f) current.Y = result.Y;
                    if (Math.Abs(result.Z - castValue.Z) > 0.001f) current.Z = result.Z;
                    
                    prop.SetValue(t, current);
                    
                    if (t is Component comp && (prop.Name == "Path" || prop.GetCustomAttribute<FilePathAttribute>() != null)) 
                        comp.UnloadAndQuit();
                }
                
                changed = true; // Still flag for item checks
            }
        }
        
        else if (prop.PropertyType == typeof(Color)) {

            var castValue = allSame ? (Color)first! : new Color(255, 255, 255, 255);
            var result = castValue.ToVector4();
            
            if (ColorEdit4(id, ref result, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar)) {
                
                newValue = result.ToColor();
                changed = true;
            }
        }
        
        else if (prop.PropertyType == typeof(int)) {

            var castValue = allSame ? (int)first! : 0;
            var result = castValue;
            
            if (InputInt(id, ref result)) {
                
                newValue = result;
                changed = true;
            }
        }
        
        else if (prop.PropertyType == typeof(bool)) {

            var castValue = allSame && (bool)first!;
            var result = castValue;
            
            if (Checkbox(id, ref result)) {
                
                newValue = result;
                changed = true;
            }
        }
        
        else if (prop.PropertyType == typeof(float)) {

            var castValue = allSame ? (float)first! : 0;
            var result = castValue;
            
            if (InputFloat(id, ref result)) {
                
                newValue = result;
                changed = true;
            }
        }
        
        if (IsItemActivated())
            foreach (var t in targets)
                History.StartRecording(t, prop.Name);

        if (changed && newValue != null) {
            
            foreach (var t in targets) {
                
                prop.SetValue(t, newValue);
                
                if (t is Component comp && (prop.Name == "Path" || prop.GetCustomAttribute<FilePathAttribute>() != null)) 
                    comp.UnloadAndQuit();
            }
        }

        if (IsItemDeactivatedAfterEdit()) History.StopRecording();
        
        PopItemWidth();
        _propIndex++;
    }
}