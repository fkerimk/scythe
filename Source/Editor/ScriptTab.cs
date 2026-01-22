using System.Numerics;

internal class ScriptTab {

    public string? FilePath;
    public string Title, Uri;
        
    [RecordHistory]
    public List<string> Lines { get; set; } = [""];
    
    [RecordHistory]
    public int CursorLine { get; set; }

    [RecordHistory]
    public int CursorChar { get; set; }
        
    [RecordHistory]
    public int SelectionStartLine { get; set; } = -1;

    [RecordHistory]
    public int SelectionStartChar { get; set; } = -1;

    public bool IsDirty, IsSelecting;

    public readonly Dictionary<int, float> LineYOffsets = new();
    public double LastRecordTime;
    public string CurrentHistoryAction = "";
        
    public Vector2 ViewPos { get; set; } = Vector2.Zero;
    public Vector2 CameraPos { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1.0f;
    public float TargetZoom { get; set; } = 1.0f;
    public float FollowCursorTimer;
        
    public readonly List<DiagnosticInfo> Diagnostics = [];
    public List<SemanticToken> SemanticTokens = [];
    public readonly HistoryStack History = new();
    public int LspVersion = 1;

    public ScriptTab(string? path) {
        
        FilePath = path;
        
        if (path != null) {
            
            Title = Path.GetFileNameWithoutExtension(path);

            var absPath = Path.GetFullPath(path);
            
            Uri = new Uri(absPath).AbsoluteUri;
            
            try {
                
                Lines = File.ReadAllLines(path).ToList();
                if (Lines.Count == 0) Lines.Add("");
                
            } catch { /**/ }
            
        } else {
            
            // Title set by NewTab
            Title = "New File";
            
            // Random Temp URI for Untitled
            Uri = new Uri(Path.Combine(Path.GetTempPath(), $"untitled_{Guid.NewGuid()}.lua")).AbsoluteUri;
        }
    }
}