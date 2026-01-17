PathUtil.LaunchPath = Environment.CurrentDirectory;

if (PathUtil.BestPath("Scythe.ini", out var scytheIniPath)) {
        
    var iniFile = new Ini(scytheIniPath);
    iniFile.ToConfig();
}
    
else throw new FileNotFoundException("Scythe.ini not found");
    
if (PathUtil.BestPath("Mod.ini", out var modIniPath)) {
        
    var iniFile = new Ini(modIniPath);
    iniFile.ToConfig();
}
    
else throw new FileNotFoundException("Mod.ini not found");

CommandLine.Init();

Script.Register();

if (!CommandLine.NoSplash) Splash.Show();

if (CommandLine.Editor) 
      Editor.Show();
else Runtime.Show();