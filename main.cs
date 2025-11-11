using scythe;

// look out for config
var ini_file = new ini(path.relative("scythe.ini"));

if (!ini_file.is_valid) {
    
    ini_file.Dispose();
    
    Directory.SetCurrentDirectory(path.exe_dir);
    ini_file = new(path.relative("scythe.ini"));
}

// config
ini_file.to_config();

if (Directory.Exists(config.mod.path)) Directory.SetCurrentDirectory(config.mod.path); else Environment.Exit(1);

// cli
cli.init();

if (!cli.get("no-splash", out _))
    new splash(1).show();

if (cli.get("editor", out _))
     new editor().show();
else new runtime().show();

Environment.Exit(0);