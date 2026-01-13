ReadConfig();

Cli.Init();

if (!Cli.Get("no-splash", out _))
    new Splash(1).Show();

if (Cli.Get("editor", out _))
     new Editor().Show();
else new Runtime().Show();

Environment.Exit(0);

return;

void ReadConfig() {
    
    var iniFile = new Ini(PathUtil.Relative("Scythe.ini"));

    if (!iniFile.IsValid) {
    
        iniFile.Dispose();
    
        Directory.SetCurrentDirectory(PathUtil.ExeDir);
        iniFile = new Ini(PathUtil.Relative("Scythe.ini"));
    }

    iniFile.ToConfig();

    if (Directory.Exists(Config.Mod.Path)) Directory.SetCurrentDirectory(Config.Mod.Path); else Environment.Exit(1);
}

