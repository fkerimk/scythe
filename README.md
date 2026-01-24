# SCYTHE 

SCYTHE is a lightweight, C#-based game engine focused on modifiability and rapid iteration using Raylib.

## 🎯 Who is this for?

- Indie developers who want full control over engine internals
- Developers interested in moddable, scriptable game architectures
- Learning-oriented projects (rendering, physics, and engine tooling)

## 🛠️ License

Scythe is licensed under the [LGPL-2.1 license](./LICENSE).

This project uses:

- [C# bindings of Raylib](https://github.com/raylib-cs/raylib-cs?tab=Zlib-1-ov-file#readme) and [C# bindings of ImGui](https://github.com/ocornut/imgui), both licensed under zlib/libpng.

- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json?tab=MIT-1-ov-file#readme) and [Jitter Physics 2 *(Jitter2)*](https://github.com/notgiven688/jitterphysics2?tab=MIT-1-ov-file#readme) both licensed under MIT.

- [MoonSharp](https://github.com/moonsharp-devs/moonsharp/?tab=License-1-ov-file#readme) licensed under the BSD 3-Clause.

## 🧱 Architecture Overview

- Component-based object model *(not ECS)*
- Shared runtime between editor and game execution

## ✨ Features

### Core

- **Modern .NET:** High-performance architecture built on the latest .NET 10 runtime.
- **Hybrid Runtime/Editor:** Combines a scene editor with a fast game runtime in a single environment.

### Graphics

- **PBR Rendering:** Shader-based Physically Based Rendering *(PBR)* support for realistic material appearance.
- **Advanced Lighting & Shadows:** Dynamic light sources with real-time Shadow Mapping.

### Physics

- **Pure C# Physics:** Jitter2 integration written entirely in C#.
- Support for dynamic and static objects.

### Editor

- **Docking UI:** ImGui-based interface with freely dockable windows.
- **Level Browser:** Hierarchical object management with a layered structure.
- **Object Browser:** Real-time panel for editing object properties, components, and physics settings.
- **JSON Level System:** Scenes and objects can be saved and loaded in a human-readable JSON format.

### Scripting

- **Lua Integration:** Fast and easy modding/scripting via MoonSharp.
- **Dynamic Input:** Simple management of mouse and keyboard input for both the editor and scripting layers.

## ❌ What this is not

- Not a full-scale AA/AAA engine.
- Not an ECS-first architecture.
- Not focused on visual scripting.

## 👷 Building

> [!NOTE]  
>  Make sure you have the .NET SDK 10.0+ packages installed.

```bash
git clone https://github.com/fkerimk/scythe.git
cd scythe
dotnet build
```

## 🏹 Running

```ps
dotnet run -- editor
```

- Opens the editor
- Press F5 to run the play mode

## 🔧 Configuring

The [Scythe.ini](./Scythe.ini) file in the project folder comes with all default settings. Scythe first reads the [Scythe.ini](./Scythe.ini) file in the working directory; if it’s not found there, it reads the one in the executable directory.

Make sure the basic resources included with the template are in the necessary locations within the project.

For more detailed configuration, you can refer to the [configuring](./CONFIGURING.md) file.

## 🙏 Attributions

#### Fonts

[Font Awesome](http://fontawesome.io) by Dave Gandy.
<br/>
[Montserrat](https://fonts.google.com/specimen/Montserrat/license) by Julieta Ulanovsky, Sol Matas, Juan Pablo del Peral, Jacques Le Bailly.
<br/>
[Cascadia Code](https://fonts.google.com/specimen/Cascadia+Code/license) by Aaron Bell, Mohamad Dakak, Viktoriya Grabowska, Liron Lavi Turkenich.

#### Samples

[Wooden Alphabet Blocks](https://skfb.ly/oRnRU) by Cherryvania, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).
<br/>
[The Green Wizard Gnome N64 Style](https://skfb.ly/oXSLR) by Drillimpact, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).
<br/>
[Animated FPS Pistol](https://skfb.ly/psqCp) by Levraicoincoin, DJMaesen, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).

#### Early Samples

[Bear Man PSX](https://skfb.ly/p9SUZ) by Bonvikt, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/).