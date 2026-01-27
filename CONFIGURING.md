# 🔧 Configuring Scythe

The [Scythe.ini](./Scythe.ini) file in the project folder comes with all default settings. Scythe first reads the [Scythe.ini](./Scythe.ini) file in the working directory; if it’s not found there, it reads the one in the executable directory.

There’s no setting you need to change to run it. You can set the location of the project to be loaded using `Mod.Path`.

```ini
[Mod]
Path="Template"
```

Make sure the basic resources [*(Template)*](./Template) included with the template are in the necessary locations within the project.

## 🚩 Mod
Project specific settings.

### Name
The name under which the project will be run.
#### Example:
```ini
Name="My Cool Project"
```

### Path
Which directory is the project located in.
#### Example:
```ini
Path="MyProject"
```

## ⚙️ Runtime / Editor
Engine’s technical settings. The same settings apply to both Runtime and Editor. Changing them is not recommended unless you know what you are doing.

### FPS
`FpsLock` is the maximum frames per second the engine will render. Setting it to `-1` matches FPS to the refresh rate *(vsync)*. Setting it to `0` makes it unlimited.
`DrawFps` toggles a text display that shows how many frames per second is rendering.
#### Example:
```ini
FpsLock=60
DrawFps=True
```

### DrawLights
Whether lights are rendered or not. Intended for debugging.
#### Example:
```ini
DrawLights=True
```

### NoShade
Whether shading is completely disabled or not.
#### Example:
```ini
NoShade=False
```

### PBR Settings
Defines how the default PBR shader renders its maps.
#### Example:
```ini
PbrAlbedo=1
PbrNormal=0
PbrMra=1
PbrEmissive=1
```

### GenTangents
Whether tangent data for 3D models is generated.
#### Example:
```ini
GenTangents=True
```

### Raylib
The logging level of the Raylib library.
#### Example:
```ini
RaylibLogLevel=Error
```

## 🗺️ Level
Contains general settings related to levels.

### Formatting
Whether level JSON files are indented. Indented files are easier to read and write, while non-indented ones take up less space.
#### Example:
```ini
Formatting=Indented
```


