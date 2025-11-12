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

path.clear_temp();

#if !DEBUG
path.include_lib(0, "raylib.dll");
path.include_lib(1, "libraylib.so");
path.include_lib(0, "cimgui.dll");
path.include_lib(1, "libcimgui.so");
#endif

if (!cli.get("no-splash", out _))
    new splash(1).show();

if (cli.get("editor", out _))
     new editor().show();
else new runtime().show();


Environment.Exit(0);

