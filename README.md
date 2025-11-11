# SCYTHE 

Just a game engine. The spiritual successor to Scythe-Unity, a Unity based game development framework (unreleased).

## 🧑‍🏫 What is this?

A simple engine designed for developing easily editable and moddable games at a basic level.

Nothing fancy. It’s not designed to take you to space. You can take a look if you want, but it’s probably not a tool you’ll need.

## 🛠️ License

Scythe is licensed under the [LGPL-2.1 license](./LICENSE).

This project uses [C# bindings](https://github.com/raylib-cs/raylib-cs) of [Raylib](https://github.com/raysan5/raylib/), both licensed under [zlib/libpng](https://github.com/raysan5/raylib/blob/master/LICENSE).

## 👷 Building

> [!CAUTION]  
> Scythe hasn’t even learned to crawl yet. It’s not mature enough to be of any real use to you. It is recommended for experimental or educational use only.

Make sure you have the .NET SDK 10.0+ packages installed.

```bash
git clone https://github.com/fkerimk/scythe.git
cd scythe
dotnet build
```

## 🏹 Running

> [!TIP]  
> You can start it without `-editor` parameter for runtime.

```ps
dotnet run -editor
```

## 🔧 Configuring

The [scythe.ini](./scythe.ini) file in the project folder comes with all default settings. Scythe first reads the [scythe.ini](./scythe.ini) file in the working directory; if it’s not found there, it reads the one in the executable directory.

There’s no setting you need to change to run it. You can set the location of the project to be loaded using `mod.path`.

```ini
[mod]
path="template"
```

Make sure the basic resources included with the template are in the necessary locations within the project.

Setting `fps_lock` to `-1` locks the FPS to the refresh rate _(vsync)_. Setting it to 0 makes it unlimited.

## 🙏 Attributions

[Font Awesome](http://fontawesome.io) by Dave Gandy.

[Bear Man PSX](https://skfb.ly/p9SUZ) by Bonvikt, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).

