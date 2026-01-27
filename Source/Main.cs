NativeResolver.Init();
PathUtil.Init();

if (PathUtil.BestPath("Scythe.ini", out var scytheIniPath)) {
    var iniFile = new Ini(scytheIniPath);
    iniFile.ToConfig();
} else
    throw new FileNotFoundException("Scythe.ini not found");

if (PathUtil.BestPath(Config.Mod.Path, out var newModPath, true)) Config.Mod.Path = newModPath;

if (PathUtil.BestPath("Mod.ini", out var modIniPath)) {
    var iniFile = new Ini(modIniPath);
    iniFile.ToConfig();
} else
    throw new FileNotFoundException("Mod.ini not found");

PathUtil.InitModPaths();

CommandLine.Init();

Script.Register();

if (CommandLine.Editor && !LspInstaller.CheckLspFiles()) CommandLine.NoSplash = false;

if (!CommandLine.NoSplash) Splash.Show();

if (CommandLine.Editor)
    Editor.Show();
else
    Runtime.Show();

return 0;