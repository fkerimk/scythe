using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using Raylib_cs;
using Newtonsoft.Json.Linq;
using static ImGuiNET.ImGui;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

internal unsafe class ScriptEditor : Viewport {
    
    #region Editor

    private const float ComboTimeout = 2.4f, RepeatInitialDelay = 0.3f, RepeatRate = 0.06f, WedgeWidth = 5.0f;

    private static float
        _shakeTime,
        _shakePower,
        _wedgeAnim,
        _repeatTimer,
        _lspUpdateTimer,
        _didChangeTimer,
        _restartTimer,
        _timer,
        _acCooldown;
    
    private static bool _isDraggingSelection, _isLeftClickPotentialDrag, _showSig, _showAutoComplete;

    private static string _dragText = "", _sigText = "", _tooltipText = "";
    
    private static int
        _lastWedgeLine = -1,
        _lastWedgeChar = -1,
        _acSelectedIndex,
        _acScrollIndex,
        _acLine,
        _acStartChar,
        _sigLine,
        _sigChar,
        _completionRequestId = -1,
        _sigRequestId = -1,
        _hoverRequestId = -1,
        _semanticTokensRequestId = -1;
    
    private static Vector2
        _shakeOffset = Vector2.Zero,
        _lastMousePos = Vector2.Zero;
    
    private static readonly List<Particle> Particles = [];

    private static KeyboardKey _lastRepeatedKey = KeyboardKey.Null;

    private static Font EditorFont => Fonts.RlCascadiaCode;
    private static List<CompletionItem> _acItems = [];
    private static readonly Random Rng = new();

    private RenderTexture2D _rt;
    [RecordHistory] private readonly List<ScriptTab> _tabs = [];
    [RecordHistory] private int _activeTabIndex = -1;
    private ScriptTab ActiveTab => _tabs[_activeTabIndex];
    private LuaLspClient? _lsp;
    private int _hoverLine = -1, _hoverChar = -1, _combo;
    private float _comboTimer, _hoverTimer;
    private bool _showDropChoice, _isExtendingHistory, _scrollToTab;
    private Component? _dropChoiceComp;
    private int _dropChoiceLine, _dropChoiceChar;
    private Vector2 _dropChoicePos;
    private HistoryStack History => ActiveTab.History;

    public ScriptEditor() : base("Script Editor") {
        
        WindowFlags |= ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        
        CustomStyle = new CustomStyle {

            WindowPadding = Vector2.Zero,
            CellPadding = Vector2.Zero,
            SeparatorTextPadding = Vector2.Zero
        };
        
        _rt = LoadRenderTexture(1, 1);
        
        InitLsp();
    }

    private static string GetPath() => PathUtil.ProjectRelative("ScriptEditor.json");

    public void Load() {
        
        var path = GetPath();
        
        if (!File.Exists(path)) return;
        
        SafeExec.Try(() => {
            
            var settings = JsonConvert.DeserializeObject<ScriptEditorSettings>(File.ReadAllText(path));
            
            if (settings == null) return;
            
            foreach (var relTabPath in settings.OpenTabs) {
                
                var normalized = relTabPath.Replace('\\', '/');
                var absPath = PathUtil.ModRelative(normalized);
                
                if (File.Exists(absPath)) NewTab(absPath);
            }
            
            if (settings.ActiveTabIndex >= 0 && settings.ActiveTabIndex < _tabs.Count)
                _activeTabIndex = settings.ActiveTabIndex;
            
            else if (_tabs.Count > 0) _activeTabIndex = 0;
        });
    }

    public void Save() {
        var path = GetPath();
        var dir = Path.GetDirectoryName(path);
        if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var settings = new ScriptEditorSettings {
            OpenTabs = _tabs.Where(t => t.FilePath != null)
                            .Select(t => Path.GetRelativePath(Config.Mod.Path, t.FilePath!).Replace('\\', '/'))
                            .ToList(),
            ActiveTabIndex = _activeTabIndex
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
    }

    private class ScriptEditorSettings {
        
        public List<string> OpenTabs { get; init; } = [];
        public int ActiveTabIndex { get; init; } = -1;
    }

    public void Open(string path) {
        
        if (_tabs.FirstOrDefault(tab => tab.FilePath == path) is { } t)
             _activeTabIndex = _tabs.IndexOf(t);
        else NewTab(path);
    }

    private void NewTab(string path) {
        
        var tab = new ScriptTab(path);
        
        _tabs.Add(tab);
        _activeTabIndex = _tabs.Count - 1;
        _scrollToTab = true;

        if (_lsp is not { IsAlive: true }) return;
        
        _lsp.SendNotification("textDocument/didOpen", new {
            
            textDocument = new {
                
                uri = tab.Uri,
                languageId = "lua",
                version = tab.LspVersion,
                text = string.Join("\n", tab.Lines)
            }
        });
        
        _lspUpdateTimer = 0.1f;
    }

    private void CloseTab(int i) {
        
        if (i < 0 || i >= _tabs.Count) return;
        
        if (_lsp is { IsAlive: true })
            _lsp.SendNotification("textDocument/didClose", new { textDocument = new { uri = _tabs[i].Uri } });
        
        _tabs.RemoveAt(i);
        
        if (_activeTabIndex >= _tabs.Count)
            _activeTabIndex = _tabs.Count - 1;
    }

    private void InitLsp() {
        
        _lsp?.Dispose();
        _lsp = null;
        
        if (!LspInstaller.CheckLspFiles()) {
            
            if (!LspInstaller.IsDone) LspInstaller.Start();
            _restartTimer = 2;
            return;
        }

        var isWin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var plat = isWin ? "Windows" : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "OSX";
        var sPath = Path.Combine(AppContext.BaseDirectory, $"External/{plat}/LuaLSP/bin", isWin ? "lua-language-server.exe" : "lua-language-server");
        
        if (!File.Exists(sPath)) return;

        _lsp = new LuaLspClient(sPath);
        
        _lsp.NotificationReceived += (m, p) => {
            
            if (m == "textDocument/publishDiagnostics")
                UpdateDiagnostics(p);
        };
        
        _lsp.ResponseReceived += (id, r) => {
            
            if (id == _completionRequestId) UpdateAutocomplete(r);
            else if (id == _sigRequestId) UpdateSignatureHelp(r);
            else if (id == _semanticTokensRequestId) UpdateSemanticTokens(r);
            else if (id == _hoverRequestId && r["contents"] is { } c)
                _tooltipText = c is JArray a
                    ? string.Join("\n", a.Select(x => x["value"]?.ToString() ?? x.ToString()))
                    : (string)(c["value"] ?? c)!;
        };
        
        _lsp.OnExited += () => {
            
            _restartTimer = 3;
            ActiveTab.SemanticTokens.Clear();
            ActiveTab.Diagnostics.Clear();
            _showAutoComplete = _showSig = false;
        };
        
        _lsp.Start().ContinueWith(_ => {
            
            if (_lsp is not { IsAlive: true }) return;
            
            foreach (var tab in _tabs)
                
                _lsp.SendNotification("textDocument/didOpen", new {
                    
                    textDocument = new {
                        
                        uri = tab.Uri,
                        languageId = "lua",
                        version = tab.LspVersion,
                        text = string.Join("\n", tab.Lines)
                    }
                });
            
            RequestSemanticTokens();
        });
    }

    private void BeginHistoryAction(string a) {
        
        if (Raylib.GetTime() - ActiveTab.LastRecordTime < 1.0 && ActiveTab.CurrentHistoryAction == a && History.CanExtend)
            _isExtendingHistory = true;
        
        else {
            
            History.StartRecording(ActiveTab, a);
            _isExtendingHistory = false;
        }

        ActiveTab.CurrentHistoryAction = a;
        ActiveTab.LastRecordTime = Raylib.GetTime();
    }

    private void EndHistoryAction() {
        
        if (_isExtendingHistory)
             History.UpdateLastRecord(ActiveTab, ActiveTab.CurrentHistoryAction);
        else History.StopRecording();
        
        _isExtendingHistory = false;
    }

    private void UpdateAutocomplete(JToken r) {
        
        _acItems.Clear();

        foreach (var i in r["items"] ?? r) {
            
            _acItems.Add(new CompletionItem {
                
                Label = (string)i["label"]!,
                InsertText = (string)(i["insertText"] ?? i["label"])!,
                InsertTextFormat = (int)(i["insertTextFormat"] ?? 1),
                Kind = (int)(i["kind"] ?? 0)
            });
        }
        
        var pref = GetCurrentWordPrefix();
        
        if (!string.IsNullOrEmpty(pref))
            _acItems = _acItems.Where(i => i.Label.StartsWith(pref, StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (_acItems.Count == 1 && _acItems[0].Label.Equals(pref, StringComparison.OrdinalIgnoreCase)) {
            
            _showAutoComplete = false;
            return;
        }

        _showAutoComplete = _acItems.Count != 0;
        _acSelectedIndex = 0;
        _acLine = ActiveTab.CursorLine;
        _acStartChar = ActiveTab.CursorChar;
    }

    private void UpdateSignatureHelp(JToken r) {
        
        if (r["signatures"] is JArray { Count: > 0 } s) {
            
            _sigText = (string)s[0]["label"]!;
            _sigLine = ActiveTab.CursorLine;
            _sigChar = ActiveTab.CursorChar;
            _showSig = true;
        }
        
        else _showSig = false;
    }

    private string GetCurrentWordPrefix() {
        
        var l = ActiveTab.Lines[ActiveTab.CursorLine];
        var s = ActiveTab.CursorChar;
        while (s > 0 && (char.IsLetterOrDigit(l[s - 1]) || l[s - 1] == '_')) s--;
        return l.Substring(s, ActiveTab.CursorChar - s);
    }

    private void UpdateDiagnostics(JToken p) {
        
        var newDiags = new List<DiagnosticInfo>();
        
        foreach (var diagnostic in p["diagnostics"] ?? Enumerable.Empty<JToken>()) {
            
            var msg = (string)diagnostic["message"]!;
            
            if (msg.Contains("lowercase initial")) continue;
            
            var r = diagnostic["range"];
            
            newDiags.Add(new DiagnosticInfo {
                
                Message = msg, Severity = (int)diagnostic["severity"]!, Line = (int)r!["start"]!["line"]!,
                StartChar = (int)r["start"]!["character"]!, EndChar = (int)r["end"]!["character"]!
            });
        }
        
        lock (ActiveTab.Diagnostics) {
            
            ActiveTab.Diagnostics.Clear();
            ActiveTab.Diagnostics.AddRange(newDiags);
        }
    }

    protected override void OnDraw() {
        
        if (ContentRegion.X <= 0 || ContentRegion.Y <= 0) return;
        
        var dt = GetFrameTime();
        
        if (_tabs.Count > 0 && _activeTabIndex >= 0) {
            
            UpdateVisualOffsets(dt);
            
            _timer += dt;
            
            if (_lspUpdateTimer > 0 && (_lspUpdateTimer -= dt) <= 0) RequestSemanticTokens();
            if (_didChangeTimer > 0 && (_didChangeTimer -= dt) <= 0) SyncChanges();
            
            if (_acCooldown > 0) _acCooldown -= dt;
            
            if (ActiveTab.CursorLine == _lastWedgeLine && ActiveTab.CursorChar == _lastWedgeChar)
                _wedgeAnim = Lerp(_wedgeAnim, 1, dt * 20);
            
            else {
                
                _wedgeAnim = Lerp(_wedgeAnim, 0, dt * 20);
                if (_wedgeAnim < 0.01f) _wedgeAnim = 0;
            }

            _lastWedgeLine = ActiveTab.CursorLine;
            _lastWedgeChar = ActiveTab.CursorChar;
            
            HandleInput();
            UpdateParticles();
            UpdateShake();
        }

        if (_restartTimer > 0 && (_restartTimer -= dt) <= 0) InitLsp();
        
        var fSize = _tabs.Count != 0 ? 20 * ActiveTab.Zoom : 20;
        var lSpace = _tabs.Count != 0 ? 26 * ActiveTab.Zoom : 26;
        var margin = _tabs.Count != 0 ? new Vector2(60, 40) * ActiveTab.Zoom : new Vector2(60, 40);
        
        PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, 7));
        Dummy(new Vector2(4, 0));
        SameLine();
        PushStyleColor(ImGuiCol.Tab, Colors.GuiTab.ToVector4());
        PushStyleColor(ImGuiCol.TabHovered, Colors.GuiTabHovered.ToVector4());
        PushStyleColor(ImGuiCol.TabSelected, Colors.GuiTabSelected.ToVector4());
        PushStyleColor(ImGuiCol.TabSelectedOverline, Colors.Primary.ToVector4());
        
        if (BeginTabBar("ScriptTabs",ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs | ImGuiTabBarFlags.FittingPolicyScroll)) {
            
            for (var i = 0; i < _tabs.Count; i++) {
                
                var open = true;
                
                if (BeginTabItem($"{(_tabs[i].IsDirty ? Icons.FaAsterisk + " " : " ")}{_tabs[i].Title} ###tab_{_tabs[i].Uri}", ref open, (_scrollToTab && i == _activeTabIndex) ? ImGuiTabItemFlags.SetSelected : 0)) {
                    
                    _activeTabIndex = i;
                    EndTabItem();
                }

                if (!open) CloseTab(i--);
            }

            EndTabBar();
            _scrollToTab = false;
        }

        PopStyleColor(4);
        Separator();
        PopStyleVar(2);
        
        if (_tabs.Count > 0 && _activeTabIndex >= 0) {
            
            var origin = GetCursorScreenPos();
            var avail = GetContentRegionAvail();
            
            if (_rt.Texture.Width != (int)avail.X || _rt.Texture.Height != (int)avail.Y) {
                
                if (_rt.Id != 0) UnloadRenderTexture(_rt);
                _rt = LoadRenderTexture((int)avail.X, (int)avail.Y);
            }

            HandleMouseSelection(origin);
            HandleCameraInput();
            UpdateCamera();
            DrawEditorContent(margin, fSize, lSpace, origin);
            Image((IntPtr)_rt.Texture.Id, avail, new Vector2(0, 1), new Vector2(1, 0));
            
            if (BeginDragDropTarget()) {
                
                if (AcceptDragDropPayload("object").NativePtr != null && LevelBrowser.DragObject is { } o) {
                    
                    var varName = char.ToLower(o.Name[0]) + o.Name[1..];
                    
                    InsertSnippetAtMouse($"local {varName} = level:find({{\"{string.Join("\", \"", o.GetPathFromRoot())}\"}})", margin, fSize, lSpace, origin, "drop_object");
                    LevelBrowser.DragObject = null;
                    SetWindowFocus("Script Editor");
                }

                if (AcceptDragDropPayload("component").NativePtr != null && LevelBrowser.DragComponent is { } c) {
                    
                    _showDropChoice = true;
                    _dropChoiceComp = c;
                    _dropChoicePos = GetIO().MousePos;
                    
                    if (GetCursorFromMouse(margin, fSize, lSpace, origin, out var tL, out var tC)) {
                        
                        _dropChoiceLine = tL;
                        _dropChoiceChar = tC;
                    }

                    OpenPopup("DropChoicePopup");
                    LevelBrowser.DragComponent = null;
                    SetWindowFocus("Script Editor");
                }

                EndDragDropTarget();
            }

            HandleDropChoice();

            // Handle Picking Drag Drop
            if (Picking.DragSource != null && Picking.IsDragging && IsHovered && IsMouseButtonReleased(MouseButton.Left)) {
                
                var o = Picking.DragSource;
                var varName = char.ToLower(o.Name[0]) + o.Name[1..];
                
                InsertSnippetAtMouse($"local {varName} = level:find({{\"{string.Join("\", \"", o.GetPathFromRoot())}\"}})", margin, fSize, lSpace, origin, "drop_object");
                Picking.DragSource = null;
                Picking.IsDragging = false;
                SetWindowFocus("Script Editor");
            }
        }
        else {
            
            var center = GetCursorScreenPos() + ContentRegion / 2;
            var text = "No Open Files";
            var tSize = MeasureTextEx(Fonts.RlMontserratRegular, text, 24, 1);
            SetCursorScreenPos(center - tSize / 2);
            PushFont(Fonts.ImMontserratRegular);
            TextColored(Colors.TextDisabled.ToVector4(), text);
            PopFont();
        }
    }

    private void AcceptCompletion(string l) {
        
        var item = _acItems.FirstOrDefault(i => i.Label == l);
        if (item.Label == null) return;
        
        BeginHistoryAction("autocomplete");
        
        var line = ActiveTab.Lines[ActiveTab.CursorLine];
        var sW = ActiveTab.CursorChar;
        
        while (sW > 0 && (char.IsLetterOrDigit(line[sW - 1]) || line[sW - 1] == '_')) sW--;
        
        var txt = item.InsertText.Replace("\t", "    ");
        var fOff = -1;
        
        if (item.InsertTextFormat == 2) {
            
            var m = Regex.Match(txt, @"\$\{\d+:?([^}]*)\}|\$\d+");
            if (m.Success) fOff = m.Index;
            txt = Regex.Replace(txt, @"\$\{\d+:?([^}]*)\}", "$1");
            txt = Regex.Replace(txt, @"\$\d+", "");
            
        } else if ((item.Kind == 2 || item.Kind == 3) && !txt.Contains('(')) {
            
            txt += "()";
            fOff = txt.Length - 1;
        }

        var sLines = txt.Replace("\r", "").Split('\n');
        var suffix = ActiveTab.Lines[ActiveTab.CursorLine][ActiveTab.CursorChar..];
        
        ActiveTab.Lines[ActiveTab.CursorLine] = ActiveTab.Lines[ActiveTab.CursorLine][..sW] + sLines[0];
        
        if (sLines.Length > 1) {
            
            for (var i = 1; i < sLines.Length; i++) ActiveTab.Lines.Insert(ActiveTab.CursorLine + i, sLines[i]);
            ActiveTab.Lines[ActiveTab.CursorLine + sLines.Length - 1] += suffix;
            
        } else ActiveTab.Lines[ActiveTab.CursorLine] += suffix;

        if (fOff != -1) {
            
            var len = 0;
            
            for (var i = 0; i < sLines.Length; i++) {
                
                if (fOff <= len + sLines[i].Length) {
                    
                    ActiveTab.CursorLine += i;
                    ActiveTab.CursorChar = (i == 0 ? sW : 0) + (fOff - len);
                    break;
                }

                len += sLines[i].Length + 1;
            }
            
        } else {
            
            ActiveTab.CursorLine += sLines.Length - 1;
            ActiveTab.CursorChar = (sLines.Length == 1 ? sW : 0) + sLines[^1].Length;
        }

        _showAutoComplete = false;
        
        EndHistoryAction();
        OnType(true, false, true, true);
    }

    private void OnType(bool major = false, bool backspace = false, bool isEnter = false, bool fromAc = false) {
        
        ActiveTab.IsDirty = true;
        ActiveTab.FollowCursorTimer = 1;
        
        _combo++;
        _comboTimer = ComboTimeout;
        
        if (major || isEnter || backspace) {
            
            _shakeTime = major ? 0.16f : 0.08f;
            _shakePower = (major ? 7.0f : 3.5f) * (backspace ? 0.6f : 1);
        }

        SpawnParticlesAtCursor(major || isEnter);
        
        _lspUpdateTimer = 0.15f;
        _didChangeTimer = 0.05f;
        
        if (_lsp is not { IsAlive: true }) return;
        
        var l = ActiveTab.Lines[ActiveTab.CursorLine];
        var pref = GetCurrentWordPrefix();
        var cB = ActiveTab.CursorChar > 0 ? l[ActiveTab.CursorChar - 1] : ' ';
        var cA = ActiveTab.CursorChar < l.Length ? l[ActiveTab.CursorChar] : ' ';
        var isT = cB is '.' or ':';
        var should = !backspace && !isEnter && !fromAc && (char.IsLetterOrDigit(cB) || cB == '_' || isT) && !(major && char.IsLetterOrDigit(cA));
        
        if (should && (pref.Length >= 1 || isT)) {
            
            SyncChanges();
            
            _completionRequestId = _lsp.SendRequest("textDocument/completion", new {
                
                textDocument = new { uri = ActiveTab.Uri },
                position = new { line = ActiveTab.CursorLine, character = ActiveTab.CursorChar },
                context = new { triggerKind = isT ? 2 : 1, triggerCharacter = isT ? cB.ToString() : null }
            });
            
        } else _showAutoComplete = false;

        if (cB == '(' || cB == ',') {
            
            SyncChanges();
            
            _sigRequestId = _lsp.SendRequest("textDocument/signatureHelp", new {
                
                textDocument = new { uri = ActiveTab.Uri },
                position = new { line = ActiveTab.CursorLine, character = ActiveTab.CursorChar }
            });
            
        } else if (isEnter || (backspace && cB == ' ') || cB == ')') _showSig = false;
    }

    private void SyncChanges() {
        
        if (_lsp is not { IsAlive: true }) return;
        
        _didChangeTimer = 0;
        
        _lsp.SendNotification("textDocument/didChange", new {
            
            textDocument = new { uri = ActiveTab.Uri, version = ActiveTab.LspVersion++ },
            contentChanges = new[] { new { text = string.Join("\n", ActiveTab.Lines) } }
        });
    }

    private void SaveActiveTab() {
        
        if (ActiveTab.FilePath == null) return;
        
        SafeExec.Try(() => {
                
            File.WriteAllText(ActiveTab.FilePath, string.Join("\n", ActiveTab.Lines));
            ActiveTab.IsDirty = false;
        });
    }

    private void RequestSemanticTokens() {
        
        if (_lsp is { IsAlive: true })
            _semanticTokensRequestId = _lsp.SendRequest("textDocument/semanticTokens/full",
                new { textDocument = new { uri = ActiveTab.Uri } });
    }

    private void UpdateSemanticTokens(JToken data) {
        
        var res = data["data"] ?? data["result"]?["data"];
        var ints = res?.ToObject<int[]>();
        
        if (ints == null) return;
        
        var tokens = new List<SemanticToken>();
        
        int l = 0, c = 0;
        
        for (var i = 0; i < ints.Length; i += 5) {
            
            l += ints[i];
            if (ints[i] > 0) c = 0;
            c += ints[i + 1];
            tokens.Add(new SemanticToken { Line = l, StartChar = c, Length = ints[i + 2], Type = ints[i + 3] });
        }

        ActiveTab.SemanticTokens = tokens;
    }

    private void HandleDropChoice() {
        
        if (!_showDropChoice || _dropChoiceComp == null) return;
        
        SetNextWindowPos(_dropChoicePos + new Vector2(10, 10));
        PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));
        PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 6));
        
        if (BeginPopup("DropChoicePopup", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)) {
            
            TextDisabled("Reference Type");
            Separator();
            
            if (Selectable("Level")) ApplyComponentDrop(false);
            if (Selectable("Self")) ApplyComponentDrop(true);
            
            if (IsKeyPressed(ImGuiKey.Escape)) {
                
                _showDropChoice = false;
                CloseCurrentPopup();
            }

            EndPopup();
        }

        PopStyleVar(2);
    }

    private void ApplyComponentDrop(bool useSelf) {
        
        if (_dropChoiceComp == null) return;
        
        var comp = _dropChoiceComp;
        var name = comp.GetType().Name;
        
        string var, snippet;
        
        if (useSelf) {
            
            var = char.ToLower(name[0]) + name[1..];
            snippet = $"local {var} = self:findComponent({{\"{name}\"}}) --[[@as {name}]]";
            
        } else {
            
            var path = new List<string>(comp.Obj.GetPathFromRoot()) { name };
            var objP = Regex.Replace(comp.Obj.Name, @"(?:^|[\s_-])(.)", m => m.Groups[1].Value.ToUpper());
            var = char.ToLower(objP[0]) + objP[1..] + name;
            snippet = $"local {var} = level:findComponent({{\"{string.Join("\", \"", path)}\"}}) --[[@as {name}]]";
        }

        ActiveTab.CursorLine = _dropChoiceLine;
        ActiveTab.CursorChar = _dropChoiceChar;
        
        BeginHistoryAction("drop_component");
        InsertTextAtCursor(snippet);
        EndHistoryAction();
        OnType(true, false, true);
        
        _showDropChoice = false;
        _dropChoiceComp = null;
        
        CloseCurrentPopup();
        SetWindowFocus("Script Editor");
    }

    private void InsertSnippetAtMouse(string s, Vector2 m, float f, float l, Vector2 o, string u) {
        
        if (!GetCursorFromMouse(m, f, l, o, out var tL, out var tC)) return;
        
        ActiveTab.CursorLine = tL;
        ActiveTab.CursorChar = tC;
        BeginHistoryAction(u);
        InsertTextAtCursor(s);
        EndHistoryAction();
        OnType(true, false, true);
    }

    #endregion

    #region Input

    private void UpdateCursorFromMouse(Vector2 m, float f, float l, Vector2 o) {
        
        ActiveTab.FollowCursorTimer = 1.0f;
        var wM = GetIO().MousePos - o + ActiveTab.CameraPos;
        ActiveTab.CursorLine = Math.Clamp((int)((wM.Y - m.Y) / l), 0, ActiveTab.Lines.Count - 1);
        var line = ActiveTab.Lines[ActiveTab.CursorLine];
        var bC = 0;
        var minD = float.MaxValue;
        
        for (var i = 0; i <= line.Length; i++) {
            
            var d = Math.Abs(wM.X - (m.X + MeasureTextEx(EditorFont, line[..i], f, 1).X));

            if (!(d < minD)) continue;
            
            minD = d;
            bC = i;
        }

        ActiveTab.CursorChar = bC;
    }

    private void HandleInput() {
        
        if (IsKeyPressed(KeyboardKey.Escape) && (LevelBrowser.DragObject != null || _isDraggingSelection)) {
            
            LevelBrowser.DragObject = LevelBrowser.DragTarget = null;
            LevelBrowser.DragComponent = null;
            LevelBrowser.IsDragCancelled = true;
            _isDraggingSelection = _isLeftClickPotentialDrag = false;
        }

        if (!IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows)) return;
        var dt = GetFrameTime();
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);

        switch (ctrl) {
            
            case true when IsKeyPressed(KeyboardKey.Z): {
                
                if (shift) History.Redo();
                else History.Undo();
                _didChangeTimer = 0.05f;
                _lspUpdateTimer = 0.1f;
                return;
            }
            
            case true when IsKeyPressed(KeyboardKey.S): SaveActiveTab(); return;
            
            case true when IsKeyPressed(KeyboardKey.Y):
                History.Redo();
                _didChangeTimer = 0.05f;
                _lspUpdateTimer = 0.1f;
                return;
            
            case true when IsKeyPressed(KeyboardKey.A):
                ActiveTab.SelectionStartLine = ActiveTab.SelectionStartChar = 0;
                ActiveTab.CursorLine = ActiveTab.Lines.Count - 1;
                ActiveTab.CursorChar = ActiveTab.Lines.Last().Length;
                _showAutoComplete = false;
                return;
            
            case true when IsKeyPressed(KeyboardKey.C): {
                
                if (HasSelection()) ImGui.SetClipboardText(GetSelectionText());
                return;
            }
            
            case true when IsKeyPressed(KeyboardKey.X): {
                
                BeginHistoryAction("cut");
                
                if (HasSelection()) {
                    
                    ImGui.SetClipboardText(GetSelectionText());
                    DeleteSelection();
                    
                } else {
                    
                    ImGui.SetClipboardText(ActiveTab.Lines[ActiveTab.CursorLine] + "\n");
                    if (ActiveTab.Lines.Count > 1) ActiveTab.Lines.RemoveAt(ActiveTab.CursorLine);
                    else ActiveTab.Lines[0] = "";
                    if (ActiveTab.CursorLine >= ActiveTab.Lines.Count) ActiveTab.CursorLine = ActiveTab.Lines.Count - 1;
                    ActiveTab.CursorChar = 0;
                    OnType(true, true, true);
                }

                EndHistoryAction();
                return;
            }
            
            case true when IsKeyPressed(KeyboardKey.V): {
                
                BeginHistoryAction("paste");
                if (HasSelection()) DeleteSelection();
                var txt = ImGui.GetClipboardText();
                InsertTextAtCursor(txt);
                if (txt.Length > 0) OnType(txt.Length > 1, false, true);
                EndHistoryAction();
                return;
            }
            
            case true when IsKeyPressed(KeyboardKey.D): {
                
                BeginHistoryAction("duplicate");
                var lS = 26 * ActiveTab.Zoom;
                
                if (HasSelection()) {
                    
                    var (sL, _, eL, eC) = GetSelectionRange();
                    var txt = GetSelectionText();
                    ActiveTab.CursorLine = eL;
                    ActiveTab.CursorChar = eC;
                    InsertTextAtCursor("\n" + txt);
                    var c = eL - sL + 1;
                    for (var i = eL + 1; i <= eL + c; i++) ActiveTab.LineYOffsets[i] = -lS * c;
                    
                }  else {
                    
                    ActiveTab.Lines.Insert(ActiveTab.CursorLine + 1, ActiveTab.Lines[ActiveTab.CursorLine]);
                    ActiveTab.LineYOffsets[ActiveTab.CursorLine + 1] = -lS;
                    ActiveTab.CursorLine++;
                }

                EndHistoryAction();
                _didChangeTimer = 0.05f;
                _lspUpdateTimer = 0.1f;
                return;
            }
        }

        if (IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt)) {
            
            var lS = 26 * ActiveTab.Zoom;
            
            if (IsKeyPressed(KeyboardKey.Up) && ActiveTab.CursorLine > 0) {
                
                BeginHistoryAction("swap");
                (ActiveTab.Lines[ActiveTab.CursorLine], ActiveTab.Lines[ActiveTab.CursorLine - 1]) = (ActiveTab.Lines[ActiveTab.CursorLine - 1], ActiveTab.Lines[ActiveTab.CursorLine]);
                ActiveTab.LineYOffsets[ActiveTab.CursorLine] = -lS;
                ActiveTab.LineYOffsets[ActiveTab.CursorLine - 1] = lS;
                ActiveTab.CursorLine--;
                if (HasSelection()) ActiveTab.SelectionStartLine--;
                EndHistoryAction();
                return;
            }

            if (IsKeyPressed(KeyboardKey.Down) && ActiveTab.CursorLine < ActiveTab.Lines.Count - 1) {
                
                BeginHistoryAction("swap");
                (ActiveTab.Lines[ActiveTab.CursorLine], ActiveTab.Lines[ActiveTab.CursorLine + 1]) = (ActiveTab.Lines[ActiveTab.CursorLine + 1], ActiveTab.Lines[ActiveTab.CursorLine]);
                ActiveTab.LineYOffsets[ActiveTab.CursorLine] = lS;
                ActiveTab.LineYOffsets[ActiveTab.CursorLine + 1] = -lS;
                ActiveTab.CursorLine++;
                if (HasSelection()) ActiveTab.SelectionStartLine++;
                EndHistoryAction();
                return;
            }
        }

        switch (ctrl) {
            
            case true when IsKeyPressed(KeyboardKey.RightBracket):
                HandleTab(false);
                return;
            
            case true when IsKeyPressed(KeyboardKey.LeftBracket):
                HandleTab(true);
                return;
        }

        KeyboardKey[] keys = [
            
            KeyboardKey.Backspace, KeyboardKey.Delete, KeyboardKey.Enter, KeyboardKey.KpEnter, KeyboardKey.Left, KeyboardKey.Right,
            KeyboardKey.Up, KeyboardKey.Down, KeyboardKey.Escape, KeyboardKey.Tab, KeyboardKey.Home, KeyboardKey.End
        ];
        
        var hit = false;
        
        foreach (var k in keys) {
            
            if (!IsKeyPressed(k)) continue;
            
            if (shift && !HasSelection()) {
                
                ActiveTab.SelectionStartLine = ActiveTab.CursorLine;
                ActiveTab.SelectionStartChar = ActiveTab.CursorChar;
            }

            PerformAction(k, shift);
            _lastRepeatedKey = k;
            _repeatTimer = 0;
            hit = true;
            break;
        }

        if (!hit && IsKeyPressed((KeyboardKey)13)) {
            
            if (shift && !HasSelection()) {
                
                ActiveTab.SelectionStartLine = ActiveTab.CursorLine;
                ActiveTab.SelectionStartChar = ActiveTab.CursorChar;
            }
            
            PerformAction(KeyboardKey.Enter, shift);
            _lastRepeatedKey = KeyboardKey.Enter;
            _repeatTimer = 0;
            hit = true;
        }

        if (!hit && _lastRepeatedKey != KeyboardKey.Null) {
            
            if (IsKeyDown(_lastRepeatedKey)) {
                
                _repeatTimer += dt;
                
                if (_repeatTimer is > RepeatInitialDelay and > RepeatInitialDelay + RepeatRate) {
                        
                    PerformAction(_lastRepeatedKey, shift);
                    _repeatTimer = RepeatInitialDelay;
                }
                
            } else {
                
                _lastRepeatedKey = KeyboardKey.Null;
                _repeatTimer = 0;
            }
        }

        var io = GetIO();
        
        for (var n = 0; n < io.InputQueueCharacters.Size; n++) {
            
            var c = (char)io.InputQueueCharacters[n];
            if (c < 32) continue;
            
            BeginHistoryAction("type");
            if (HasSelection()) DeleteSelection();
            var add = c.ToString();
            
            if (
                ActiveTab.CursorChar < ActiveTab.Lines[ActiveTab.CursorLine].Length &&
                ActiveTab.Lines[ActiveTab.CursorLine][ActiveTab.CursorChar] == c &&
                c is ')' or '}' or ']' or '"' or '\''
            ) {
                ActiveTab.CursorChar++;
                OnType();
                EndHistoryAction();
                continue;
            }

            add = c switch {
                
                '(' => "()",
                '{' => "{}",
                '[' => "[]",
                '"' => "\"\"",
                '\'' => "''",
                _ => add
            };
            
            ActiveTab.Lines[ActiveTab.CursorLine] = ActiveTab.Lines[ActiveTab.CursorLine].Insert(ActiveTab.CursorChar, add);
            ActiveTab.CursorChar++;
            
            OnType();
            EndHistoryAction();
        }
    }

    private void PerformAction(KeyboardKey k, bool s) {
        
        ActiveTab.FollowCursorTimer = 1.0f;
        
        if (k == KeyboardKey.Escape) {
            
            _showAutoComplete = _showSig = false;
            _acSelectedIndex = -1;
            _hoverTimer = 0;
            _tooltipText = "";
            _isDraggingSelection = _isLeftClickPotentialDrag = false;
            LevelBrowser.DragObject = LevelBrowser.DragTarget = null;
            return;
        }

        if (_showAutoComplete && _acItems.Count > 0 && !HasSelection()) {
            
            switch (k) {
                
                case KeyboardKey.Down:
                    _acSelectedIndex = (_acSelectedIndex + 1) % _acItems.Count;
                    return;
                
                case KeyboardKey.Up:
                    _acSelectedIndex = (_acSelectedIndex - 1 + _acItems.Count) % _acItems.Count;
                    return;
                
                case KeyboardKey.Enter or KeyboardKey.Tab when _acSelectedIndex != -1:
                    AcceptCompletion(_acItems[_acSelectedIndex].Label);
                    return;
            }
        }

        if (k == KeyboardKey.Left || k == KeyboardKey.Right || k == KeyboardKey.Up || k == KeyboardKey.Down)
            _showAutoComplete = false;
        
        if (
            !s &&
            k != KeyboardKey.Backspace &&
            k != KeyboardKey.Delete &&
            k != KeyboardKey.Enter &&
            k != KeyboardKey.KpEnter &&
            k != KeyboardKey.Tab
            && HasSelection()
        ) {
            var (sL, sC, eL, eC) = GetSelectionRange();
            
            if (k == KeyboardKey.Left || k == KeyboardKey.Up) {
                
                ActiveTab.CursorLine = sL;
                ActiveTab.CursorChar = sC;
                
            } else {
                
                ActiveTab.CursorLine = eL;
                ActiveTab.CursorChar = eC;
            }

            ClearSelection();
            return;
        }

        switch (k) {
            
            case KeyboardKey.Backspace:
                
                BeginHistoryAction("backspace");
                
                if (HasSelection()) DeleteSelection(); else {
                    
                    if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl)) {
                        
                        var oL = ActiveTab.CursorLine;
                        var oC = ActiveTab.CursorChar;
                        MoveCursorWordLeft();
                        ActiveTab.SelectionStartLine = ActiveTab.CursorLine;
                        ActiveTab.SelectionStartChar = ActiveTab.CursorChar;
                        ActiveTab.CursorLine = oL;
                        ActiveTab.CursorChar = oC;
                        DeleteSelection();
                        
                    } else HandleBackspace();
                }

                EndHistoryAction();
                break;
            
            case KeyboardKey.Delete:
                
                BeginHistoryAction("delete");
                
                if (HasSelection()) DeleteSelection(); else {
                    
                    if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl)) {
                        
                        var oL = ActiveTab.CursorLine;
                        var oC = ActiveTab.CursorChar;
                        MoveCursorWordRight();
                        ActiveTab.SelectionStartLine = oL;
                        ActiveTab.SelectionStartChar = oC;
                        DeleteSelection();
                        
                    } else {
                        
                        if (ActiveTab.CursorChar < ActiveTab.Lines[ActiveTab.CursorLine].Length) {
                            
                            ActiveTab.Lines[ActiveTab.CursorLine] = ActiveTab.Lines[ActiveTab.CursorLine].Remove(ActiveTab.CursorChar, 1);
                            OnType(false, true);
                            
                        } else if (ActiveTab.CursorLine < ActiveTab.Lines.Count - 1) {
                            
                            ActiveTab.Lines[ActiveTab.CursorLine] += ActiveTab.Lines[ActiveTab.CursorLine + 1];
                            ActiveTab.Lines.RemoveAt(ActiveTab.CursorLine + 1);
                            OnType(true, true);
                        }
                    }
                }

                EndHistoryAction();
                break;
            
            case KeyboardKey.Enter:
            case KeyboardKey.KpEnter:
                BeginHistoryAction("enter");
                if (HasSelection()) DeleteSelection();
                HandleEnter();
                EndHistoryAction();
                break;
            
            case KeyboardKey.Tab:
                BeginHistoryAction("tab");
                _showAutoComplete = false;
                HandleTab(IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift));
                EndHistoryAction();
                break;
            
            case KeyboardKey.Left:
                if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl)) MoveCursorWordLeft();
                else MoveCursorLeft();
                break;
            
            case KeyboardKey.Right:
                if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl)) MoveCursorWordRight();
                else MoveCursorRight();
                break;
            
            case KeyboardKey.Up: MoveCursorUp(); break;
            case KeyboardKey.Down: MoveCursorDown(); break;
            case KeyboardKey.Home:
                if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl)) 
                     ActiveTab.CursorLine = ActiveTab.CursorChar = 0;
                else ActiveTab.CursorChar = 0;

                break;
            
            case KeyboardKey.End:
                
                if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl)) {
                    
                    ActiveTab.CursorLine = ActiveTab.Lines.Count - 1;
                    ActiveTab.CursorChar = ActiveTab.Lines[ActiveTab.CursorLine].Length;
                } 
                
                else ActiveTab.CursorChar = ActiveTab.Lines[ActiveTab.CursorLine].Length;

                break;
        }

        if (HasSelection()) _showAutoComplete = false;
        if (!s && k != KeyboardKey.Backspace && k != KeyboardKey.Delete && k != KeyboardKey.Enter &&
            k != KeyboardKey.Tab && HasSelection()) ClearSelection();
    }

    private void HandleTab(bool s) {
        
        if (HasSelection()) {
            
            var (sL, _, eL, eC) = GetSelectionRange();
            
            if (eL > sL && eC == 0) eL--;
            
            for (var i = sL; i <= eL; i++) {
                
                if (s) {
                    
                    if (ActiveTab.Lines[i].StartsWith("    ")) {
                        ActiveTab.Lines[i] = ActiveTab.Lines[i][4..];
                        if (i == ActiveTab.CursorLine) ActiveTab.CursorChar = Math.Max(0, ActiveTab.CursorChar - 4);
                        if (i == ActiveTab.SelectionStartLine)
                            ActiveTab.SelectionStartChar = Math.Max(0, ActiveTab.SelectionStartChar - 4);
                    }
                    else if (ActiveTab.Lines[i].StartsWith("\t")) {
                        ActiveTab.Lines[i] = ActiveTab.Lines[i][1..];
                        if (i == ActiveTab.CursorLine) ActiveTab.CursorChar = Math.Max(0, ActiveTab.CursorChar - 1);
                        if (i == ActiveTab.SelectionStartLine)
                            ActiveTab.SelectionStartChar = Math.Max(0, ActiveTab.SelectionStartChar - 1);
                    }
                }
                else {
                    ActiveTab.Lines[i] = "    " + ActiveTab.Lines[i];
                    if (i == ActiveTab.CursorLine) ActiveTab.CursorChar += 4;
                    if (i == ActiveTab.SelectionStartLine) ActiveTab.SelectionStartChar += 4;
                }
            }

            OnType();
        }
        else {
            if (s) {
                if (ActiveTab.Lines[ActiveTab.CursorLine].StartsWith("    ")) {
                    ActiveTab.Lines[ActiveTab.CursorLine] = ActiveTab.Lines[ActiveTab.CursorLine][4..];
                    ActiveTab.CursorChar = Math.Max(0, ActiveTab.CursorChar - 4);
                }
                else if (ActiveTab.Lines[ActiveTab.CursorLine].StartsWith("\t")) {
                    ActiveTab.Lines[ActiveTab.CursorLine] = ActiveTab.Lines[ActiveTab.CursorLine][1..];
                    ActiveTab.CursorChar = Math.Max(0, ActiveTab.CursorChar - 1);
                }
            }
            else {
                ActiveTab.Lines[ActiveTab.CursorLine] =
                    ActiveTab.Lines[ActiveTab.CursorLine].Insert(ActiveTab.CursorChar, "    ");
                ActiveTab.CursorChar += 4;
            }

            OnType();
        }
    }

    private void MoveCursorWordLeft() {
        if (ActiveTab.CursorChar == 0) {
            if (ActiveTab.CursorLine > 0) {
                ActiveTab.CursorLine--;
                ActiveTab.CursorChar = ActiveTab.Lines[ActiveTab.CursorLine].Length;
            }

            return;
        }

        var l = ActiveTab.Lines[ActiveTab.CursorLine];
        var i = ActiveTab.CursorChar;

        // 1. Skip whitespace backwards
        while (i > 0 && char.IsWhiteSpace(l[i - 1])) i--;

        if (i > 0) {
            var isWord = char.IsLetterOrDigit(l[i - 1]) || l[i - 1] == '_';
            if (isWord) {
                // 2. Skip word characters backwards
                while (i > 0 && (char.IsLetterOrDigit(l[i - 1]) || l[i - 1] == '_')) i--;
            }
            else {
                // 3. Skip symbol characters backwards
                while (i > 0 && !char.IsLetterOrDigit(l[i - 1]) && l[i - 1] != '_' && !char.IsWhiteSpace(l[i - 1])) i--;
            }
        }

        ActiveTab.CursorChar = i;
    }

    private void MoveCursorWordRight() {
        var l = ActiveTab.Lines[ActiveTab.CursorLine];
        if (ActiveTab.CursorChar >= l.Length) {
            if (ActiveTab.CursorLine < ActiveTab.Lines.Count - 1) {
                ActiveTab.CursorLine++;
                ActiveTab.CursorChar = 0;
            }

            return;
        }

        var i = ActiveTab.CursorChar;

        // 1. Skip whitespace forwards
        while (i < l.Length && char.IsWhiteSpace(l[i])) i++;

        if (i < l.Length) {
            var isWord = char.IsLetterOrDigit(l[i]) || l[i] == '_';
            if (isWord) {
                // 2. Skip word characters forwards
                while (i < l.Length && (char.IsLetterOrDigit(l[i]) || l[i] == '_')) i++;
            }
            else {
                // 3. Skip symbol characters forwards
                while (i < l.Length && !char.IsLetterOrDigit(l[i]) && l[i] != '_' && !char.IsWhiteSpace(l[i])) i++;
            }
        }

        ActiveTab.CursorChar = i;
    }

    private bool HasSelection() => ActiveTab.SelectionStartLine != -1;

    private void ClearSelection() {
        ActiveTab.SelectionStartLine = ActiveTab.SelectionStartChar = -1;
    }

    private (int sL, int sC, int eL, int eC) GetSelectionRange() => !HasSelection()
        ? (0, 0, 0, 0)
        :
        (ActiveTab.SelectionStartLine < ActiveTab.CursorLine || (ActiveTab.SelectionStartLine == ActiveTab.CursorLine &&
                                                                 ActiveTab.SelectionStartChar <=
                                                                 ActiveTab.CursorChar))
            ?
            (ActiveTab.SelectionStartLine, ActiveTab.SelectionStartChar, ActiveTab.CursorLine, ActiveTab.CursorChar)
            : (ActiveTab.CursorLine, ActiveTab.CursorChar, ActiveTab.SelectionStartLine, ActiveTab.SelectionStartChar);

    private void SelectWordAtCursor() {
        var l = ActiveTab.Lines[ActiveTab.CursorLine];
        var p = ActiveTab.CursorChar;
        if (l.Length == 0) return;
        var s = p;
        var e = p;
        while (s > 0 && (char.IsLetterOrDigit(l[s - 1]) || l[s - 1] == '_')) s--;
        while (e < l.Length && (char.IsLetterOrDigit(l[e]) || l[e] == '_')) e++;
        if (s == e) {
            if (e < l.Length) e++;
            else if (s > 0) s--;
        }

        ActiveTab.SelectionStartLine = ActiveTab.CursorLine;
        ActiveTab.SelectionStartChar = s;
        ActiveTab.CursorChar = e;
    }

    private void DeleteSelection() {
        var (sL, sC, eL, eC) = GetSelectionRange();
        if (sL == eL) ActiveTab.Lines[sL] = ActiveTab.Lines[sL].Remove(sC, eC - sC);
        else {
            ActiveTab.Lines[sL] = ActiveTab.Lines[sL][..sC] + ActiveTab.Lines[eL][eC..];
            ActiveTab.Lines.RemoveRange(sL + 1, eL - sL);
        }

        ActiveTab.CursorLine = sL;
        ActiveTab.CursorChar = sC;
        ClearSelection();
        OnType(false, true);
    }

    private void HandleEnter() {
        if (HasSelection()) DeleteSelection();
        ClampCursor();
        var l = ActiveTab.Lines[ActiveTab.CursorLine];
        var left = l[..ActiveTab.CursorChar];
        var right = l[ActiveTab.CursorChar..];
        var indent = "";
        foreach (var c in left)
            if (char.IsWhiteSpace(c)) indent += c;
            else break;
        if (left.TrimEnd().EndsWith("do") || left.TrimEnd().EndsWith("then") || left.TrimEnd().EndsWith("function()") ||
            left.TrimEnd().EndsWith("{")) indent += "    ";
        ActiveTab.Lines[ActiveTab.CursorLine] = left;
        ActiveTab.Lines.Insert(ActiveTab.CursorLine + 1, indent + right);
        ActiveTab.CursorLine++;
        ActiveTab.CursorChar = indent.Length;
        ClampCursor();
        OnType(false, false, true);
    }

    private void HandleBackspace() {
        if (HasSelection()) {
            DeleteSelection();
            return;
        }

        if (ActiveTab.CursorChar > 0) {
            var l = ActiveTab.Lines[ActiveTab.CursorLine];
            var pD = false;
            if (ActiveTab.CursorChar < l.Length) {
                var cL = l[ActiveTab.CursorChar - 1];
                var cR = l[ActiveTab.CursorChar];
                if ((cL == '(' && cR == ')') || (cL == '{' && cR == '}') || (cL == '[' && cR == ']') ||
                    (cL == '"' && cR == '"') || (cL == '\'' && cR == '\'')) {
                    ActiveTab.Lines[ActiveTab.CursorLine] = l.Remove(ActiveTab.CursorChar - 1, 2);
                    ActiveTab.CursorChar--;
                    pD = true;
                }
            }

            if (!pD) {
                ActiveTab.Lines[ActiveTab.CursorLine] = l.Remove(ActiveTab.CursorChar - 1, 1);
                ActiveTab.CursorChar--;
            }

            OnType(false, true);
        }
        else if (ActiveTab.CursorLine > 0) {
            ActiveTab.CursorChar = ActiveTab.Lines[ActiveTab.CursorLine - 1].Length;
            ActiveTab.Lines[ActiveTab.CursorLine - 1] += ActiveTab.Lines[ActiveTab.CursorLine];
            ActiveTab.Lines.RemoveAt(ActiveTab.CursorLine);
            ActiveTab.CursorLine--;
            OnType(true, true);
        }
    }

    private void MoveCursorLeft() {
        if (ActiveTab.CursorChar > 0) ActiveTab.CursorChar--;
        else if (ActiveTab.CursorLine > 0) {
            ActiveTab.CursorLine--;
            ActiveTab.CursorChar = ActiveTab.Lines[ActiveTab.CursorLine].Length;
        }
    }

    private void MoveCursorRight() {
        if (ActiveTab.CursorChar < ActiveTab.Lines[ActiveTab.CursorLine].Length) ActiveTab.CursorChar++;
        else if (ActiveTab.CursorLine < ActiveTab.Lines.Count - 1) {
            ActiveTab.CursorLine++;
            ActiveTab.CursorChar = 0;
        }
    }

    private void MoveCursorUp() {
        if (ActiveTab.CursorLine > 0) {
            ActiveTab.CursorLine--;
            ActiveTab.CursorChar = Math.Min(ActiveTab.CursorChar, ActiveTab.Lines[ActiveTab.CursorLine].Length);
        }
    }

    private void MoveCursorDown() {
        if (ActiveTab.CursorLine < ActiveTab.Lines.Count - 1) {
            ActiveTab.CursorLine++;
            ActiveTab.CursorChar = Math.Min(ActiveTab.CursorChar, ActiveTab.Lines[ActiveTab.CursorLine].Length);
        }
    }

    private void InsertTextAtCursor(string t) {
        if (string.IsNullOrEmpty(t)) return;
        ClampCursor();
        var l = t.Replace("\r", "").Split('\n');
        var suffix = ActiveTab.Lines[ActiveTab.CursorLine][ActiveTab.CursorChar..];
        ActiveTab.Lines[ActiveTab.CursorLine] = ActiveTab.Lines[ActiveTab.CursorLine][..ActiveTab.CursorChar] + l[0];
        if (l.Length > 1) {
            for (var i = 1; i < l.Length; i++) ActiveTab.Lines.Insert(ActiveTab.CursorLine + i, l[i]);
            ActiveTab.CursorLine += l.Length - 1;
            ActiveTab.CursorChar = l[^1].Length;
            ActiveTab.Lines[ActiveTab.CursorLine] += suffix;
        }
        else {
            ActiveTab.CursorChar += l[0].Length;
            ActiveTab.Lines[ActiveTab.CursorLine] += suffix;
        }

        ClampCursor();
    }

    private void ClampCursor() {
        if (ActiveTab.Lines.Count == 0) ActiveTab.Lines.Add("");
        ActiveTab.CursorLine = Math.Clamp(ActiveTab.CursorLine, 0, ActiveTab.Lines.Count - 1);
        ActiveTab.CursorChar = Math.Clamp(ActiveTab.CursorChar, 0, ActiveTab.Lines[ActiveTab.CursorLine].Length);
        if (ActiveTab.SelectionStartLine != -1) {
            ActiveTab.SelectionStartLine = Math.Clamp(ActiveTab.SelectionStartLine, 0, ActiveTab.Lines.Count - 1);
            ActiveTab.SelectionStartChar = Math.Clamp(ActiveTab.SelectionStartChar, 0,
                ActiveTab.Lines[ActiveTab.SelectionStartLine].Length);
        }
    }

    private string GetSelectionText() {
        if (!HasSelection()) return "";
        var (sL, sC, eL, eC) = GetSelectionRange();
        if (sL == eL) return ActiveTab.Lines[sL].Substring(sC, eC - sC);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(ActiveTab.Lines[sL][sC..]);
        for (var i = sL + 1; i < eL; i++) sb.AppendLine(ActiveTab.Lines[i]);
        sb.Append(ActiveTab.Lines[eL][..eC]);
        return sb.ToString();
    }

    private void HandleMouseSelection(Vector2 o) {
        if (!IsHovered) return;
        var f = 20 * ActiveTab.Zoom;
        var l = 26 * ActiveTab.Zoom;
        var m = new Vector2(60, 40) * ActiveTab.Zoom;
        var wM = GetIO().MousePos - o + ActiveTab.CameraPos;
        if (IsMouseDoubleClicked(ImGuiMouseButton.Left)) {
            UpdateCursorFromMouse(m, f, l, o);
            SelectWordAtCursor();
            ActiveTab.IsSelecting = false;
            _isLeftClickPotentialDrag = false;
            return;
        }

        if (IsMouseButtonPressed(MouseButton.Left)) {
            if (HasSelection() && IsInsideSelection(wM, m, f, l)) _isLeftClickPotentialDrag = true;
            else {
                UpdateCursorFromMouse(m, f, l, o);
                _showAutoComplete = _showSig = false;
                _hoverTimer = 0;
                _tooltipText = "";
                if (!IsKeyDown(KeyboardKey.LeftShift) && !IsKeyDown(KeyboardKey.RightShift)) {
                    ActiveTab.SelectionStartLine = ActiveTab.CursorLine;
                    ActiveTab.SelectionStartChar = ActiveTab.CursorChar;
                }

                ActiveTab.IsSelecting = true;
                _isLeftClickPotentialDrag = false;
            }
        }

        if (IsMouseButtonDown(MouseButton.Left)) {
            if (_isLeftClickPotentialDrag && Vector2.Distance(GetIO().MouseDelta, Vector2.Zero) > 0.1f) {
                _isDraggingSelection = true;
                _isLeftClickPotentialDrag = false;
                _dragText = GetSelectionText();
            }

            if (ActiveTab.IsSelecting) UpdateCursorFromMouse(m, f, l, o);
        }

        if (IsMouseButtonReleased(MouseButton.Left)) {
            if (_isDraggingSelection && GetCursorFromMouse(m, f, l, o, out var tL, out var tC)) {
                var (sL, sC, eL, eC) = GetSelectionRange();
                var inside = tL > sL && tL < eL;
                if (sL == eL) {
                    if (tL == sL && tC >= sC && tC <= eC) inside = true;
                }
                else {
                    if (tL == sL && tC >= sC) inside = true;
                    if (tL == eL && tC <= eC) inside = true;
                }

                if (!inside) {
                    BeginHistoryAction("drag");
                    var txt = _dragText;
                    DeleteSelection();
                    if (sL == eL && tL == sL) {
                        if (tC > eC) tC -= (eC - sC);
                    }
                    else if (tL == eL && sL != eL) {
                        tL = sL;
                        tC = sC + (tC - eC);
                    }
                    else if (tL > sL) tL -= (eL - sL);

                    ActiveTab.CursorLine = tL;
                    ActiveTab.CursorChar = tC;
                    InsertTextAtCursor(txt);
                    EndHistoryAction();
                }
            }
            else if (_isLeftClickPotentialDrag) {
                UpdateCursorFromMouse(m, f, l, o);
                ClearSelection();
            }

            _isDraggingSelection = false;
            _dragText = "";
            ActiveTab.IsSelecting = false;
            if (ActiveTab.SelectionStartLine == ActiveTab.CursorLine &&
                ActiveTab.SelectionStartChar == ActiveTab.CursorChar) ClearSelection();
            _isLeftClickPotentialDrag = false;
        }

        if (_isLeftClickPotentialDrag && IsMouseButtonDown(MouseButton.Left) &&
            Vector2.Distance(GetIO().MouseDelta, Vector2.Zero) > 0.1f) {
            _isDraggingSelection = true;
            _dragText = GetSelectionText();
            _isLeftClickPotentialDrag = false;
        }
    }

    #endregion

    #region Rendering

    private void DrawEditorContent(Vector2 m, float fS, float lS, Vector2 sO) {
        if (_comboTimer > 0 && (_comboTimer -= GetFrameTime()) <= 0) _combo = 0;
        var hM = GetIO().MousePos;
        if (Vector2.Distance(hM, _lastMousePos) > 5) {
            _hoverTimer = 0;
            _tooltipText = "";
            _lastMousePos = hM;
        }
        else if (IsHovered && (_hoverTimer += GetFrameTime()) > 0.05f && string.IsNullOrEmpty(_tooltipText)) {
            var wM = hM - sO + ActiveTab.CameraPos;
            var hL = (int)Math.Max(0, (wM.Y - m.Y) / lS);
            if (hL >= 0 && hL < ActiveTab.Lines.Count) {
                var l = ActiveTab.Lines[hL];
                var hC = -1;
                var minD = 30 * ActiveTab.Zoom;
                for (var i = 0; i <= l.Length; i++) {
                    var x = m.X + MeasureTextEx(EditorFont, l[..i], fS, 1).X;
                    var d = Math.Abs(wM.X - x);
                    if (d < minD) {
                        minD = d;
                        hC = i;
                    }
                }

                if (hC != -1 && (hL != _hoverLine || hC != _hoverChar)) {
                    _hoverLine = hL;
                    _hoverChar = hC;
                    _tooltipText = "";
                    var diag = ActiveTab.Diagnostics.FirstOrDefault(d =>
                        d.Line == hL && hC >= d.StartChar && hC <= d.EndChar);
                    if (diag.Message != null) _tooltipText = diag.Message;
                    else if (_lsp is { IsAlive: true })
                        _hoverRequestId = _lsp.SendRequest("textDocument/hover",
                            new {
                                textDocument = new { uri = ActiveTab.Uri }, position = new { line = hL, character = hC }
                            });
                }
            }
        }

        BeginTextureMode(_rt);
        ClearBackground(new Color(15, 15, 20, 255));
        foreach (var p in Particles) {
            var sP = p.Position - ActiveTab.CameraPos + _shakeOffset;
            DrawCircleV(sP, p.Size * ActiveTab.Zoom, p.Color);
            if (p.Size * ActiveTab.Zoom > 2)
                DrawCircleV(sP, p.Size * 1.5f * ActiveTab.Zoom, Fade(p.Color, 0.1f * (p.Life / 0.6f)));
        }

        DrawBackgroundGrid();
        var bP = m - ActiveTab.CameraPos + _shakeOffset;
        DrawSelection(fS, lS, bP);
        if ((int)(_timer * 2.4f) % 2 == 0 || ActiveTab.IsSelecting || _wedgeAnim < 0.9f) {
            var cp = GetCursorWorldPosition(m, fS, lS);
            var sCp = cp - ActiveTab.CameraPos + _shakeOffset;
            DrawCircleGradient((int)sCp.X + 1, (int)(sCp.Y + fS / 2), (int)(15 * ActiveTab.Zoom),
                Fade(GetComboColor(_combo), 0.4f), Color.Blank);
        }

        for (var i = 0; i < ActiveTab.Lines.Count; i++) {
            var yO = i * lS + ActiveTab.LineYOffsets.GetValueOrDefault(i, 0);
            if (yO + bP.Y + lS < 0 || yO + bP.Y > ContentRegion.Y) continue;
            var lNt = (i + 1).ToString();
            var lNs = MeasureTextEx(EditorFont, lNt, fS, 1);
            var lNp = new Vector2(m.X - lNs.X - 15 * ActiveTab.Zoom, bP.Y + yO);
            var diags = ActiveTab.Diagnostics.Where(d => d.Line == i).ToList();
            var lNc = i == ActiveTab.CursorLine ? new Color(180, 180, 255, 255) : new Color(70, 70, 90, 255);
            if (diags.Count != 0) {
                var w = diags.OrderBy(d => d.Severity).First();
                lNc = w.Severity switch {
                    1 => Color.Red,
                    2 => Color.Orange,
                    _ => Color.SkyBlue
                };
                DrawCircleV(lNp + new Vector2(-15 * ActiveTab.Zoom, lNs.Y / 2), 3 * ActiveTab.Zoom, lNc);
            }

            DrawTextEx(EditorFont, lNt, lNp, fS, 1, lNc);
            DrawHybridHighlightedLine(i, bP + new Vector2(0, yO), fS, i == ActiveTab.CursorLine);
        }

        if (_isDraggingSelection) DrawDragSelectionGhost(m, fS, lS, bP, sO);
        if (LevelBrowser.DragObject != null) DrawLevelDragGhost(m, fS, lS, bP, sO, LevelBrowser.DragObject, false);
        if (LevelBrowser.DragComponent != null) DrawLevelDragGhost(m, fS, lS, bP, sO, LevelBrowser.DragComponent.Obj, true);
        if (Picking.DragSource != null && Picking.IsDragging) DrawLevelDragGhost(m, fS, lS, bP, sO, Picking.DragSource, false);
        if (_showSig) DrawSignatureHelp(m, fS, lS);
        if (_showAutoComplete) DrawAutocompletePopup(m, fS, lS, sO);
        DrawTooltip(hM - sO);
        DrawLspStatus();
        DrawComboUi();
        EndTextureMode();
    }

    private void DrawDragSelectionGhost(Vector2 m, float fS, float lS, Vector2 bP, Vector2 sO) {
        var dP = GetMousePosition() - sO;
        var lines = _dragText.Replace("\r", "").Split('\n');
        var gM = 20.0f;
        var gW = lines.Select(l => MeasureTextEx(EditorFont, l, fS, 1).X).Max() + gM * 2;
        var gH = lines.Length * fS + gM;
        var gP = dP + new Vector2(25, 10);
        DrawRectangleRounded(new Rectangle(gP.X, gP.Y, gW, gH), 0.1f, 8, new Color(20, 20, 30, 200));
        DrawRectangleRoundedLines(new Rectangle(gP.X, gP.Y, gW, gH), 0.1f, 8, new Color(100, 100, 120, 150));
        for (var i = 0; i < lines.Length; i++)
            DrawTextEx(EditorFont, lines[i], gP + new Vector2(gM, gM / 2 + i * fS), fS, 1, Color.White);
        if (GetCursorFromMouse(m, fS, lS, sO, out var tL, out var tC)) {
            var (sL, sC, eL, eC) = GetSelectionRange();
            if (!((tL > sL && tL < eL) || (tL == sL && tC > sC && (tL != eL)) || (tL == eL && tC < eC && (tL != sL)) ||
                  (sL == eL && tL == sL && tC >= sC && tC <= eC)))
                DrawDropMarker(
                    bP + new Vector2(
                        tL < ActiveTab.Lines.Count
                            ? MeasureTextEx(EditorFont, ActiveTab.Lines[tL][..Math.Min(tC, ActiveTab.Lines[tL].Length)],
                                fS, 1).X
                            : 0, tL * lS), fS, Color.Yellow);
        }
    }

    private void DrawLevelDragGhost(Vector2 m, float fS, float lS, Vector2 bP, Vector2 sO, Obj obj, bool comp) {
        var txt = comp
            ? $"level:findComponent({{\"{string.Join("\", \"", obj.GetPathFromRoot().Append(LevelBrowser.DragComponent?.GetType().Name ?? ""))}\"}})"
            : $"level:find({{\"{string.Join("\", \"", obj.GetPathFromRoot())}\"}})";
        var dP = GetMousePosition() - sO;
        var gP = dP + new Vector2(25, 10);
        var gS = MeasureTextEx(EditorFont, txt, fS, 1);
        var gPa = 12.0f;
        var c = new Color(130, 200, 255, 255);
        DrawRectangleRounded(new Rectangle(gP.X, gP.Y, gS.X + gPa * 2, gS.Y + gPa), 0.2f, 8,
            new Color(25, 25, 40, 220));
        DrawRectangleRoundedLines(new Rectangle(gP.X, gP.Y, gS.X + gPa * 2, gS.Y + gPa), 0.2f, 8, Fade(c, 0.6f));
        DrawRectangleV(gP, new Vector2(4, gS.Y + gPa), c);
        DrawTextEx(EditorFont, txt, gP + new Vector2(gPa + 4, gPa / 2), fS, 1, Color.White);
        DrawCircleV(dP, 5 * ActiveTab.Zoom, c);
        if (GetCursorFromMouse(m, fS, lS, sO, out var tL, out var tC))
            DrawDropMarker(
                bP + new Vector2(
                    tL < ActiveTab.Lines.Count
                        ? MeasureTextEx(EditorFont, ActiveTab.Lines[tL][..Math.Min(tC, ActiveTab.Lines[tL].Length)], fS,
                            1).X
                        : 0, tL * lS), fS, c);
    }

    private void DrawDropMarker(Vector2 p, float fS, Color c) {
        DrawRectangleV(p, new Vector2(2, fS), c);
        DrawCircleV(p, 3, c);
        DrawCircleV(p + new Vector2(0, fS), 3, c);
    }

    private void DrawBackgroundGrid() {
        var s = 40;
        var c = new Color(25, 25, 30, 255);
        var oX = -(int)(ActiveTab.CameraPos.X * 0.2f) % s;
        var oY = -(int)(ActiveTab.CameraPos.Y * 0.2f) % s;
        for (var x = oX; x < ContentRegion.X; x += s) DrawRectangle(x, 0, 1, (int)ContentRegion.Y, c);
        for (var y = oY; y < ContentRegion.Y; y += s) DrawRectangle(0, y, (int)ContentRegion.X, 1, c);
    }

    private void DrawSelection(float fS, float lS, Vector2 bP) {
        if (!HasSelection()) return;
        var (sL, sC, eL, eC) = GetSelectionRange();
        var c = new Color(100, 180, 255, 80);
        var wW = WedgeWidth * ActiveTab.Zoom * _wedgeAnim;
        for (var i = sL; i <= eL; i++) {
            if (i >= ActiveTab.Lines.Count) break;
            var l = ActiveTab.Lines[i];
            var y = bP.Y + i * lS;
            if (y + lS < 0 || y > ContentRegion.Y) continue;
            var x1 = bP.X + (i == sL ? MeasureTextEx(EditorFont, l[..Math.Clamp(sC, 0, l.Length)], fS, 1).X : 0);
            if (ActiveTab.CursorLine == i && sC > ActiveTab.CursorChar) x1 += wW;
            var x2 = bP.X + (i == eL
                ? MeasureTextEx(EditorFont, l[..Math.Clamp(eC, 0, l.Length)], fS, 1).X
                : MeasureTextEx(EditorFont, l, fS, 1).X + MeasureTextEx(EditorFont, " ", fS, 1).X);
            if (ActiveTab.CursorLine == i && eC > ActiveTab.CursorChar) x2 += wW;
            DrawRectangleV(new Vector2(x1, y), new Vector2(Math.Max(5, x2 - x1), fS), c);
        }
    }

    private void DrawHybridHighlightedLine(int lI, Vector2 p, float fS, bool a) {
        var l = ActiveTab.Lines[lI];
        var curX = p.X;
        if (string.IsNullOrEmpty(l)) {
            DrawFormattedText("", ref curX, p.Y, fS, Color.White, a, lI, 0, p.X);
            return;
        }

        var tokens = ActiveTab.SemanticTokens.Where(t => t.Line == lI).OrderBy(t => t.StartChar).ToList();
        var last = 0;
        foreach (var token in tokens) {
            var s = Math.Clamp(token.StartChar, last, l.Length);
            if (s > last) {
                DrawRegexChunk(l[last..s], ref curX, p.Y, fS, a, lI, last, p.X);
                last = s;
            }

            if (last >= l.Length) break;
            var len = Math.Min(token.Length, l.Length - last);
            if (len <= 0) continue;
            DrawFormattedText(l.Substring(last, len), ref curX, p.Y, fS, token.GetLspColor(), a, lI, last, p.X);
            last += len;
        }

        if (last < l.Length) DrawRegexChunk(l[last..], ref curX, p.Y, fS, a, lI, last, p.X);

        DiagnosticInfo[] diags;
        lock (ActiveTab.Diagnostics) diags = ActiveTab.Diagnostics.Where(d => d.Line == lI).ToArray();
        
        foreach (var d in diags) {
            var x1 = p.X + MeasureTextEx(EditorFont, l[..Math.Min(l.Length, d.StartChar)], fS, 1).X;
            var x2 = p.X + MeasureTextEx(EditorFont, l[..Math.Min(l.Length, d.EndChar)], fS, 1).X;
            DrawWavyLine(new Vector2(x1, p.Y + fS + 2 * ActiveTab.Zoom), new Vector2(x2, p.Y + fS + 2 * ActiveTab.Zoom),
                d.Severity == 1 ? Color.Red : (d.Severity == 2 ? Color.Orange : Color.SkyBlue));
        }
    }

    private void DrawWavyLine(Vector2 s, Vector2 e, Color c) {
        var wL = 12f * ActiveTab.Zoom;
        var wA = 1.5f * ActiveTab.Zoom;
        var t = 2f * ActiveTab.Zoom;
        var segments = (int)Math.Max(1, (e.X - s.X) / 2f);
        for (var i = 0; i < segments; i++) {
            var x1 = s.X + i * 2f;
            var x2 = Math.Min(s.X + (i + 1) * 2f, e.X);
            var y1 = s.Y + (float)Math.Sin((x1 + _timer * 8) / wL * Math.PI * 2) * wA;
            var y2 = s.Y + (float)Math.Sin((x2 + _timer * 8) / wL * Math.PI * 2) * wA;
            DrawLineEx(new Vector2(x1, y1), new Vector2(x2, y2), t, c);
        }
    }

    private void DrawRegexChunk(string line, ref float x, float y, float f, bool a, int lI, int sC, float lS) {
        
        foreach (var token in line.Tokenize()) {
            
            DrawFormattedText(token.Text, ref x, y, f, token.GetColor(), a, lI, sC, lS);
            sC += token.Text.Length;
        }
    }

    private void DrawFormattedText(string t, ref float cX, float y, float f, Color c, bool a, int lI, int sC,
        float lS) {
        if (a) c = ColorAlphaBlend(c, Color.White, Fade(Color.White, 0.2f));
        var lF = ActiveTab.Lines[lI];
        var wW = WedgeWidth * ActiveTab.Zoom * _wedgeAnim;
        var cC = GetComboColor(_combo);
        for (var i = 0; i <= t.Length; i++) {
            var idx = sC + i;
            var tX = lS + MeasureTextEx(EditorFont, lF[..idx], f, 1f).X +
                     (ActiveTab.CursorLine == lI && idx > ActiveTab.CursorChar ? wW : 0);
            if (ActiveTab.CursorLine == lI && ActiveTab.CursorChar == idx) {
                if (((int)(_timer * 2.4f) % 2 == 0 || ActiveTab.IsSelecting || _wedgeAnim < 0.9f) &&
                    (i < t.Length || idx == lF.Length))
                    DrawRectangleV(new Vector2(tX, y), new Vector2(2.5f * ActiveTab.Zoom, f), cC);
                tX += wW;
            }

            if (i == t.Length) {
                cX = tX;
                break;
            }

            var rot = (ActiveTab.CursorLine == lI && ActiveTab.CursorChar == idx) ? WedgeWidth * _wedgeAnim : 0;
            var s = t[i].ToString();
            var p = new Vector2(tX, y);
            if (_combo > 50 && c.R > 100)
                DrawTextPro(EditorFont, s, p + new Vector2(1, 1), Vector2.Zero, rot, f, 1f, Fade(c, 0.3f));
            DrawTextPro(EditorFont, s, p, Vector2.Zero, rot, f, 1f, c);
            cX = tX + MeasureTextEx(EditorFont, s, f, 1f).X;
        }
    }

    private void DrawLspStatus() {
        if (_lsp != null) {
            var c = _lsp.Status == "Connected"
                ? Color.Lime
                : (_lsp.Status.StartsWith("Error") ? Color.Red : Color.Gold);
            var txt = $"LSP: {_lsp.Status}";
            var s = MeasureTextEx(EditorFont, txt, 14, 1);
            DrawTextEx(EditorFont, txt, new Vector2(ContentRegion.X - s.X - 10, ContentRegion.Y - 20), 14, 1, c);
        }
    }

    private void UpdateVisualOffsets(float dt) {
        foreach (var k in ActiveTab.LineYOffsets.Keys.ToList()) {
            ActiveTab.LineYOffsets[k] = Lerp(ActiveTab.LineYOffsets[k], 0, dt * 24f);
            if (Math.Abs(ActiveTab.LineYOffsets[k]) < 0.1f) ActiveTab.LineYOffsets.Remove(k);
        }
    }

    private Vector2 GetCursorWorldPosition(Vector2 m, float f, float l) => m + new Vector2(
        MeasureTextEx(EditorFont,
            ActiveTab.Lines[Math.Clamp(ActiveTab.CursorLine, 0, ActiveTab.Lines.Count - 1)][
                ..Math.Clamp(ActiveTab.CursorChar, 0,
                    ActiveTab.Lines[Math.Clamp(ActiveTab.CursorLine, 0, ActiveTab.Lines.Count - 1)].Length)], f, 1).X,
        Math.Clamp(ActiveTab.CursorLine, 0, ActiveTab.Lines.Count - 1) * l);

    private void UpdateParticles() {
        var dt = GetFrameTime();
        for (var i = Particles.Count - 1; i >= 0; i--) {
            var p = Particles[i];
            p.Life -= dt;
            if (p.Life <= 0) {
                Particles.RemoveAt(i);
                continue;
            }

            p.Position += p.Velocity * dt;
            p.Velocity.Y += 350 * dt * ActiveTab.Zoom;
            Particles[i] = p;
        }
    }

    private void UpdateShake() {
        if (_shakeTime > 0) {
            _shakeTime -= GetFrameTime();
            _shakeOffset = new Vector2((float)(Rng.NextDouble() * 2 - 1) * _shakePower,
                (float)(Rng.NextDouble() * 2 - 1) * _shakePower) * ActiveTab.Zoom;
        }
        else _shakeOffset = Vector2.Zero;
    }

    private void SpawnParticlesAtCursor(bool m) {
        var f = 20 * ActiveTab.Zoom;
        var l = 26 * ActiveTab.Zoom;
        var ma = new Vector2(60, 40) * ActiveTab.Zoom;
        var p = GetCursorWorldPosition(ma, f, l);
        var count = m ? 12 : 3;
        var c = GetComboColor(_combo);
        for (var i = 0; i < count; i++) {
            var a = (float)(Rng.NextDouble() * Math.PI * 2);
            var s = (float)(Rng.NextDouble() * 100 + (m ? 120 : 40)) * ActiveTab.Zoom;
            Particles.Add(new Particle {
                Position = p + new Vector2(0, f * 0.5f),
                Velocity = new Vector2((float)Math.Cos(a) * s, (float)Math.Sin(a) * s - 40 * ActiveTab.Zoom), Color = c,
                Size = (float)Rng.NextDouble() * 3 + 1, Life = (float)Rng.NextDouble() * 0.45f + 0.25f
            });
        }
    }

    private void DrawComboUi() {
        if (_combo <= 5) return;
        var txt = $"COMBO x{_combo}";
        var c = GetComboColor(_combo);
        var s = MeasureTextEx(EditorFont, txt, 24, 1);
        var p = new Vector2(ContentRegion.X - s.X - 30, 30);
        DrawTextEx(EditorFont, txt, p + new Vector2(2, 2), 24, 1, Fade(c, 0.3f));
        DrawTextEx(EditorFont, txt, p, 24, 1, Color.White);
        var bP = new Vector2(p.X, p.Y + s.Y + 6);
        DrawRectangleV(bP, new Vector2(s.X, 3f), Fade(Color.Gray, 0.2f));
        DrawRectangleV(bP, new Vector2(s.X * (_comboTimer / ComboTimeout), 3f), c);
    }

    private void UpdateCamera() {
        var dt = GetFrameTime();
        if (ActiveTab.FollowCursorTimer > 0) ActiveTab.FollowCursorTimer -= dt;
        var f = 20 * ActiveTab.Zoom;
        var l = 26 * ActiveTab.Zoom;
        var m = new Vector2(60, 40) * ActiveTab.Zoom;
        var wC = GetCursorWorldPosition(m, f, l);
        var vW = ContentRegion.X;
        var vH = ContentRegion.Y;
        var v = ActiveTab.ViewPos;
        if (ActiveTab.FollowCursorTimer > 0) {
            if (wC.X < v.X + m.X + 20 * ActiveTab.Zoom) v.X = wC.X - m.X - 20 * ActiveTab.Zoom;
            if (wC.X + 20 > v.X + vW - 40 * ActiveTab.Zoom) v.X = wC.X + 20 - (vW - 40 * ActiveTab.Zoom);
            if (wC.Y < v.Y + m.Y) v.Y = wC.Y - m.Y;
            if (wC.Y + l > v.Y + vH - m.Y) v.Y = wC.Y + l - (vH - m.Y);
            ActiveTab.ViewPos = v;
        }

        ActiveTab.CameraPos = Vector2.Lerp(ActiveTab.CameraPos, ActiveTab.ViewPos, dt * 12f);
        ActiveTab.Zoom = Lerp(ActiveTab.Zoom, ActiveTab.TargetZoom, dt * 12f);
    }

    private void HandleCameraInput() {
        if (!IsHovered) return;
        if (IsMouseButtonDown(MouseButton.Middle)) ActiveTab.ViewPos -= GetMouseDelta();
        var w = GetMouseWheelMove();
        if (w == 0) return;
        if (IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl))
            ActiveTab.TargetZoom = Math.Clamp(ActiveTab.TargetZoom + w * 0.12f * ActiveTab.TargetZoom, 0.1f, 5.0f);
        else {
            var v = ActiveTab.ViewPos;
            if (IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift)) v.X -= w * 80 * ActiveTab.Zoom;
            else v.Y -= w * 80 * ActiveTab.Zoom;
            ActiveTab.ViewPos = v;
            ActiveTab.FollowCursorTimer = 0;
        }
    }

    private void DrawAutocompletePopup(Vector2 m, float fS, float lS, Vector2 sO) {
        if (ActiveTab.CursorLine != _acLine || Math.Abs(ActiveTab.CursorChar - _acStartChar) > 10) {
            _showAutoComplete = false;
            return;
        }

        var cW = GetCursorWorldPosition(m, fS, lS);
        var pP = cW - ActiveTab.CameraPos + new Vector2(0, fS + 5);
        var w = 320 * ActiveTab.Zoom;
        var iH = 24 * ActiveTab.Zoom;
        var mV = 10;
        var h = Math.Min(mV * iH + 10 * ActiveTab.Zoom, _acItems.Count * iH + 10 * ActiveTab.Zoom);
        if (pP.X + w > _rt.Texture.Width) pP.X = _rt.Texture.Width - w - 10;
        if (pP.Y + h > _rt.Texture.Height) pP.Y = cW.Y - ActiveTab.CameraPos.Y - h - 10;
        pP.X = Math.Max(10, pP.X);
        pP.Y = Math.Max(10, pP.Y);
        if (_acSelectedIndex < _acScrollIndex) _acScrollIndex = _acSelectedIndex;
        if (_acSelectedIndex >= _acScrollIndex + mV) _acScrollIndex = _acSelectedIndex - mV + 1;
        DrawRectangleRounded(new Rectangle(pP.X, pP.Y, w, h), 0.15f, 8, new Color(20, 20, 30, 245));
        DrawRectangleRoundedLines(new Rectangle(pP.X, pP.Y, w, h), 0.15f, 8, new Color(80, 80, 100, 150));
        var mR = GetIO().MousePos - sO;
        for (var i = _acScrollIndex; i < Math.Min(_acScrollIndex + mV, _acItems.Count); i++) {
            var item = _acItems[i];
            var iY = pP.Y + 5 * ActiveTab.Zoom + (i - _acScrollIndex) * iH;
            var iR = new Rectangle(pP.X + 5 * ActiveTab.Zoom, iY, w - 10 * ActiveTab.Zoom, iH);
            var hov = CheckCollisionPointRec(mR, iR);
            var sel = i == _acSelectedIndex;
            if (hov && Vector2.Distance(GetIO().MouseDelta, Vector2.Zero) > 0.1f) _acSelectedIndex = i;
            if (hov && IsMouseButtonPressed(MouseButton.Left)) {
                AcceptCompletion(item.Label);
                return;
            }

            if (sel)
                DrawRectangleRounded(iR, 0.2f, 8, hov ? new Color(100, 130, 255, 150) : new Color(80, 110, 230, 100));
            var iCv = GetAcIconColor(item.Kind);
            var iC = new Color((byte)(iCv.X * 255), (byte)(iCv.Y * 255), (byte)(iCv.Z * 255), (byte)255);
            DrawCircleV(new Vector2(pP.X + 15 * ActiveTab.Zoom, iY + iH / 2), 4 * ActiveTab.Zoom, iC);
            var l = item.Label;
            var mTw = w - 40 * ActiveTab.Zoom;
            if (MeasureTextEx(EditorFont, l, 18 * ActiveTab.Zoom, 1).X > mTw) {
                while (l.Length > 3 && MeasureTextEx(EditorFont, l + "...", 18 * ActiveTab.Zoom, 1).X > mTw)
                    l = l[..^1];
                l += "...";
            }

            DrawTextEx(EditorFont, l, new Vector2(pP.X + 30 * ActiveTab.Zoom, iY + (iH - 18 * ActiveTab.Zoom) / 2),
                18 * ActiveTab.Zoom, 1, sel ? Color.White : iC);
        }
    }

    private void DrawSignatureHelp(Vector2 m, float fS, float lS) {
        if (ActiveTab.CursorLine != _sigLine || ActiveTab.CursorChar < _sigChar) {
            _showSig = false;
            return;
        }

        var cW = GetCursorWorldPosition(m, fS, lS);
        var sP = cW - ActiveTab.CameraPos + new Vector2(0, -fS - 15 * ActiveTab.Zoom);
        var s = MeasureTextEx(EditorFont, _sigText, 18 * ActiveTab.Zoom, 1);
        var p = 8 * ActiveTab.Zoom;
        DrawRectangleRounded(new Rectangle(sP.X - p, sP.Y - p, s.X + p * 2, s.Y + p * 2), 0.2f, 8,
            new Color(25, 25, 40, 240));
        DrawRectangleRoundedLines(new Rectangle(sP.X - p, sP.Y - p, s.X + p * 2, s.Y + p * 2), 0.2f, 8,
            new Color(100, 200, 255, 100));
        DrawTextEx(EditorFont, _sigText, sP, 18 * ActiveTab.Zoom, 1, new Color(100, 200, 255, 255));
    }

    private void DrawTooltip(Vector2 mO) {
        if (string.IsNullOrEmpty(_tooltipText) || _hoverTimer < 0.05f) return;
        var tP = mO + new Vector2(15, 15);
        var f = 16 * ActiveTab.Zoom;
        var lines = _tooltipText.Split('\n');
        var mW = Math.Min(600 * ActiveTab.Zoom,
            lines.Select(l => MeasureTextEx(EditorFont, l.Replace("```lua", "").Replace("```", ""), f, 1).X).Prepend(0)
                .Max() + 24 * ActiveTab.Zoom);
        var h = lines.Length * (f + 6) + 16 * ActiveTab.Zoom;
        if (tP.X + mW > _rt.Texture.Width) tP.X = mO.X - mW - 10;
        if (tP.Y + h > _rt.Texture.Height) tP.Y = mO.Y - h - 10;
        tP.X = Math.Max(2, tP.X);
        tP.Y = Math.Max(2, tP.Y);
        DrawRectangleRounded(new Rectangle(tP.X, tP.Y, mW, h), 0.1f, 8, new Color(20, 20, 28, 252));
        DrawRectangleRoundedLines(new Rectangle(tP.X, tP.Y, mW, h), 0.1f, 8, new Color(80, 80, 110, 150));
        DrawRectangleV(tP, new Vector2(mW, 3 * ActiveTab.Zoom), GetComboColor(_combo));
        
        var code = false;
        
        for (var i = 0; i < lines.Length; i++) {
            
            var dP = tP + new Vector2(12 * ActiveTab.Zoom, 10 * ActiveTab.Zoom + i * (f + 6));
            
            if (lines[i].StartsWith("```lua")) {
                
                code = true;
                continue;
            }

            if (lines[i].StartsWith("```")) {
                
                code = false;
                continue;
            }

            if (code) {
                
                var curX = dP.X;
                
                foreach (var token in lines[i].Tokenize()) {
                    
                    DrawTextEx(EditorFont, token.Text, dP with { X = curX }, f, 1, token.GetColor());
                    curX += MeasureTextEx(EditorFont, token.Text, f, 1).X;
                }
                
            } else {
                
                var color = lines[i].Contains("error", StringComparison.OrdinalIgnoreCase)
                    ? Color.Red
                    : (lines[i].Contains("warning", StringComparison.OrdinalIgnoreCase)
                        ? Color.Orange
                        : (lines[i].StartsWith("(global)") || lines[i].StartsWith("(local)")
                            ? new Color(130, 200, 255, 255)
                            : Color.White));
                
                DrawTextEx(EditorFont, lines[i], dP, f, 1, color);
            }
        }
    }

    private bool IsInsideSelection(Vector2 wM, Vector2 m, float f, float l) {
        var (sL, sC, eL, eC) = GetSelectionRange();
        var line = (int)Math.Max(0, (wM.Y - m.Y) / l);
        if (line < sL || line > eL) return false;
        var lT = ActiveTab.Lines[line];
        var sX = m.X + (line == sL ? MeasureTextEx(EditorFont, lT[..sC], f, 1).X : 0);
        var eX = m.X +
                 (line == eL ? MeasureTextEx(EditorFont, lT[..eC], f, 1).X : MeasureTextEx(EditorFont, lT, f, 1).X);
        return wM.X >= sX && wM.X <= eX;
    }

    private bool GetCursorFromMouse(Vector2 m, float f, float l, Vector2 sO, out int line, out int chars) {
        var wM = GetIO().MousePos - sO + ActiveTab.CameraPos;
        line = Math.Clamp((int)((wM.Y - m.Y) / l), 0, ActiveTab.Lines.Count - 1);
        var lT = ActiveTab.Lines[line];
        var minD = float.MaxValue;
        chars = lT.Length;
        for (var i = 0; i <= lT.Length; i++) {
            var d = Math.Abs(wM.X - (m.X + MeasureTextEx(EditorFont, lT[..i], f, 1).X));
            if (d < minD) {
                minD = d;
                chars = i;
            }
        }

        return true;
    }

    private static Color GetComboColor(int c) => c switch {
        < 20 => Color.SkyBlue, < 50 => Color.Lime, < 100 => Color.Gold, < 200 => Color.Orange, < 500 => Color.Red,
        _ => Color.Magenta
    };

    private static Vector4 GetAcIconColor(int k) => k switch {
        
        2 or 3 => new Vector4(1f, .8f, .2f, 1f), 6 => new Vector4(.2f, .7f, 1f, 1f),
        12 or 13 or 21 => new Vector4(.4f, 1f, .6f, 1f), 5 or 7 or 9 => new Vector4(1f, .6f, .4f, 1f),
        14 => new Vector4(1f, .4f, .7f, 1f), _ => new Vector4(.7f, .8f, 1f, 1f)
    };

    #endregion
}