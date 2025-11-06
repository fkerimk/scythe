using Raylib_cs;
using scythe;

Directory.SetCurrentDirectory(path.exe_dir);

splash:
//var splash = new splash(2); splash.show(
//    320, 190, 
//    TraceLogLevel.None, 
//    ConfigFlags.UndecoratedWindow
//);

editor:
var editor = new editor(); editor.show(
    1, 1, 
    TraceLogLevel.Warning, 
    ConfigFlags.Msaa4xHint,
    ConfigFlags.AlwaysRunWindow,
    ConfigFlags.ResizableWindow
);

runtime:
var runtime = new runtime();