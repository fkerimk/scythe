# SCYTHE 

Just a game engine. The spiritual successor to Scythe-Unity, a Unity based game development framework (unreleased).

## 🧑‍🏫 What is this?

A simple engine designed for developing easily editable and moddable games at a basic level.

Nothing fancy. It’s not designed to take you to space. You can take a look if you want, but it’s probably not a tool you’ll need.

## 🛠️ License

Scythe is licensed under the [LGPL-2.1 license](./LICENSE).

This project uses [C# bindings](https://github.com/raylib-cs/raylib-cs) of [Raylib](https://github.com/raysan5/raylib/) and [C# bindings](https://github.com/raylib-extras/rlImGui-cs) of [ImGui](https://github.com/ocornut/imgui), all licensed under [zlib/libpng](https://github.com/raysan5/raylib/blob/master/LICENSE).

## 👷 Building

> [!CAUTION]  
> Scythe is still in its infancy. It’s not mature enough to be of much real use. It is recommended for experimental or educational use only.

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

The [Scythe.ini](./Scythe.ini) file in the project folder comes with all default settings. Scythe first reads the [Scythe.ini](./Scythe.ini) file in the working directory; if it’s not found there, it reads the one in the executable directory.

Make sure the basic resources included with the template are in the necessary locations within the project.

For more detailed configuration, you can refer to the [configuring](./CONFIGURING.md) file.

## 🙏 Attributions

[Font Awesome](http://fontawesome.io) by Dave Gandy.

[Bear Man PSX](https://skfb.ly/p9SUZ) by Bonvikt, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).

[The Green Wizard Gnome N64 Style](https://skfb.ly/oXSLR) by Drillimpact, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).
