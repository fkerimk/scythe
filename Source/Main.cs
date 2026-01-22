PathUtil.Init();

foreach (var arg in args) {
    
    if (!arg.StartsWith("cfg:")) continue;
    
    var setting = arg[4..];
    var opIndex = setting.IndexOf("!=", StringComparison.Ordinal);
    
    if (opIndex <= 0) continue;
        
    var key = setting[..opIndex].Trim();
    var val = setting[(opIndex + 2)..].Trim();
        
    Config.Set(key, val, true);
}

if (PathUtil.BestPath("Scythe.ini", out var scytheIniPath)) {
        
    var iniFile = new Ini(scytheIniPath);
    iniFile.ToConfig();
}
    
else throw new FileNotFoundException("Scythe.ini not found");

if (PathUtil.BestPath(Config.Mod.Path, out var newModPath, true))
    Config.Mod.Path = newModPath;

if (PathUtil.BestPath("Mod.ini", out var modIniPath)) {
        
    var iniFile = new Ini(modIniPath);
    iniFile.ToConfig();
}
    
else throw new FileNotFoundException("Mod.ini not found");

foreach (var arg in args) {
    
    if (!arg.StartsWith("cfg:")) continue;
    
    var setting = arg[4..];
    var eqIndex = setting.IndexOf('=');
    
    if (eqIndex <= 0) continue;
        
    var key = setting[..eqIndex].Trim();
    var val = setting[(eqIndex + 1)..].Trim();
        
    Config.Set(key, val);
}

CommandLine.Init();

Script.Register();

if (CommandLine.Editor && !LspInstaller.CheckLspFiles()) CommandLine.NoSplash = false;

if (!CommandLine.NoSplash) Splash.Show();

if (CommandLine.Editor) 
      Editor.Show();
else Runtime.Show();