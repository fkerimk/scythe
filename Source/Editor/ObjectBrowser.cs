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
    private object? _pickerTarget;
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

        if (LevelBrowser.SelectedObject == null) return;
        if (Core.ActiveLevel == null) return;

        if (LevelBrowser.SelectedObject.Parent != null) {

            PushStyleColor(ImGuiCol.Text, Colors.GuiTextDisabled.ToVector4());
            Text(LevelBrowser.SelectedObject.Parent.Name);
            PopStyleColor();
            SameLine();
        }

        Separator();
        Spacing();

        DrawProperties(LevelBrowser.SelectedObject, false, null);
        DrawProperties(LevelBrowser.SelectedObject.Transform, true, "Transform");

        var components = LevelBrowser.SelectedObject.Components.Values.OrderBy(c => c.GetType().Name, new NaturalStringComparer());

        foreach (var component in components)
            DrawProperties(component, true, component.GetType().Name);

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

                if (LevelBrowser.SelectedObject.Components.ContainsKey(type.Name)) {

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
                        try { p.SetValue(newComponent, p.GetValue(component)); } catch { /**/ }
                    }
                    
                    targetObj.Components[compName] = newComponent;
                });
                
                History.StopRecording();

                if (component is Animation animation &&  LevelBrowser.SelectedObject.Components.TryGetValue("Model", out var model))
                    animation.Path = (model as Model)!.Path;
            }
            
            EndPopup();
        }

        DrawFilePicker();
    }

    private void DrawFilePicker() {
        if (_shouldOpenPicker) {
            OpenPopup("File Picker");
            _shouldOpenPicker = false;
        }

        if (BeginPopup("File Picker")) {
            SetNextItemWidth(300);
            if (InputTextWithHint("##filter", "Search...", ref _searchFilter, 128)) {
                // Filter is handled by just checking contains below
            }

            BeginChild("##files", new Vector2(300, 400));
            foreach (var file in _foundFiles) {
                if (!string.IsNullOrEmpty(_searchFilter) && !file.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (Selectable(file)) {
                    if (_pickerTarget != null && _pickerProp != null) {
                        History.StartRecording(_pickerTarget, _pickerProp.Name);
                        _pickerProp.SetValue(_pickerTarget, file);
                        History.StopRecording();

                        if (_pickerTarget is Component comp) comp.UnloadAndQuit();
                    }
                    CloseCurrentPopup();
                }
            }
            EndChild();

            EndPopup();
        }
    }

    private void DrawProperties(object obj, bool separator, string? title) {
        
        PushID(obj.GetHashCode());
        
        if (separator && !string.IsNullOrEmpty(title)) {
            
            Spacing();
            
            var headerPos = GetCursorScreenPos();
            var headerSize = new Vector2(GetContentRegionAvail().X, GetFrameHeight());
            
            // Background
            GetWindowDrawList().AddRectFilled(headerPos, headerPos + headerSize, GetColorU32(ImGuiCol.Header, 0.45f), 2.0f);
            
            PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 3));
            PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
            
            var treeOpen = TreeNodeEx($"##{title}_header", ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth);
            
            // Drag Source (Attached to the header row)
            if (obj is Component comp && BeginDragDropSource()) {
                LevelBrowser._dragComponent = comp;
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
            
            if (obj is Component c) {

                PushFont(Fonts.ImFontAwesomeSmall);
                TextColored(c.LabelColor.ToVector4(), c.LabelIcon);
                PopFont();
                SameLine();
            }
            
            ImGui.PushFont(Fonts.ImMontserratRegular);
            Text(title);
            ImGui.PopFont();

            if (obj is Component component and not Transform) {
                
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
                            try { p.SetValue(newComponent, p.GetValue(component)); } catch { /**/ }
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

                foreach (var prop in obj.GetType().GetProperties()) {
                    
                    var headAttr = prop.GetCustomAttribute<HeaderAttribute>();
                    if (headAttr != null) {
                        Columns(1);
                        Spacing();
                        ImGui.PushFont(Fonts.ImMontserratRegular);
                        var headerCp = ImGui.GetCursorPos();
                        TextColored(Colors.Primary.ToVector4(), headAttr.Title);
                        ImGui.SetCursorPos(headerCp + new Vector2(0.3f, 0));
                        TextColored(Colors.Primary.ToVector4(), headAttr.Title);
                        ImGui.PopFont();
                        Separator();
                        Spacing();
                        Columns(2, "##props", false);
                        SetColumnWidth(0, GetWindowWidth() * 0.3f);
                    }

                    var labelAttr = prop.GetCustomAttribute<LabelAttribute>();
                    if (labelAttr == null) continue;

                    AlignTextToFramePadding();
                    ImGui.PushFont(Fonts.ImMontserratRegular);
                    var labelCp = ImGui.GetCursorPos();
                    Text(labelAttr.Value);
                    ImGui.SetCursorPos(labelCp + new Vector2(0.3f, 0));
                    Text(labelAttr.Value);
                    ImGui.PopFont();
                    NextColumn();
                    
                    DrawProperty(obj, prop);
                    NextColumn();
                }
                    
                Columns(1);
                PopStyleVar();

                TreePop();
                Spacing();
            }
        } 
        else {
             // Non-separated properties (e.g. main object)
             PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, 8));
             Columns(2, "##props", false);
             SetColumnWidth(0, GetWindowWidth() * 0.3f); 

             foreach (var prop in obj.GetType().GetProperties()) {
                var labelAttr = prop.GetCustomAttribute<LabelAttribute>();
                if (labelAttr == null) continue;

                AlignTextToFramePadding();
                ImGui.PushFont(Fonts.ImMontserratRegular);
                var labelAltCp = ImGui.GetCursorPos();
                Text(labelAttr.Value);
                ImGui.SetCursorPos(labelAltCp + new Vector2(0.3f, 0));
                Text(labelAttr.Value);
                ImGui.PopFont();
                NextColumn();
                DrawProperty(obj, prop);
                NextColumn();
             }
             Columns(1);
             PopStyleVar();
        }
        
        PopID();
    }

    private void DrawProperty(object target, PropertyInfo prop) {

        var id = $"##prop{_propIndex}";
        var value = prop.GetValue(target);
            
        if (value != null) {
                
            var changed = false;
            object? newValue = null;
            
            PushItemWidth(-1);

            if (prop.PropertyType == typeof(string)) {

                var castValue = (string)value;
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
                                
                                _pickerTarget = target;
                                _pickerProp = prop;
                                _searchFilter = "";
                                _shouldOpenPicker = true;
                            }
                        }
                        
                        else Notifications.Show($"Folder not found: {filePathAttr.Category}");
                    }
                    SameLine();
                }

                if (InputTextWithHint(id, "object", ref castValue, 512)) {
                    
                    newValue = castValue;
                    changed = true;
                }
            }
            
            else if (prop.PropertyType == typeof(Vector3)) {

                var castValue = (Vector3)value;
                var convertedValue = castValue;
                
                if (InputFloat3(id, ref convertedValue)) {
                    
                    newValue = convertedValue;
                    changed = true;
                }
            }
            
            else if (prop.PropertyType == typeof(Color)) {

                var castValue = (Color)value;
                var convertedValue = castValue.ToVector4();
                
                if (ColorEdit4(id, ref convertedValue, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar)) {
                    
                    newValue = convertedValue.ToColor();
                    changed = true;
                }
            }
            
            else if (prop.PropertyType == typeof(int)) {

                var castValue = (int)value;
                
                if (InputInt(id, ref castValue)) {
                    
                    newValue = castValue;
                    changed = true;
                }
            }
            
            else if (prop.PropertyType == typeof(bool)) {

                var castValue = (bool)value;
                
                if (Checkbox(id, ref castValue)) {
                    
                    newValue = castValue;
                    changed = true;
                }
            }
            
            else if (prop.PropertyType == typeof(float)) {

                var castValue = (float)value;
                
                if (InputFloat(id, ref castValue)) {
                    
                    newValue = castValue;
                    changed = true;
                }
            }
            
            if (IsItemActivated()) History.StartRecording(target, prop.Name);
            
            if (changed && newValue != null) {
                
                prop.SetValue(target, newValue);
                
                if (target is Component comp && (prop.Name == "Path" || prop.GetCustomAttribute<FilePathAttribute>() != null)) 
                    comp.UnloadAndQuit();
            }
            
            if (IsItemDeactivatedAfterEdit()) History.StopRecording();

            PopItemWidth();
        }
        
        _propIndex++;
    }
}