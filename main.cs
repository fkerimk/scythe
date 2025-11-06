using Raylib_cs;
using scythe;

Directory.SetCurrentDirectory(path.exe_dir);

var splash = false;
var editor = true;

if (splash) {
    
    new splash(2).show(
        320, 190, 
        TraceLogLevel.None, 
        ConfigFlags.UndecoratedWindow
    );
}

if (editor) {
    
    new editor().show(
        1, 1, 
        TraceLogLevel.Warning, 
        ConfigFlags.Msaa4xHint,
        ConfigFlags.AlwaysRunWindow,
        ConfigFlags.ResizableWindow
    );
    
} else {
    
    new runtime().show(
        1, 1, 
        TraceLogLevel.Warning, 
        ConfigFlags.Msaa4xHint,
        ConfigFlags.AlwaysRunWindow,
        ConfigFlags.ResizableWindow
    );
}

Environment.Exit(0);