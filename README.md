# SCYTHE <img width="24" height="24" alt="icon" src="https://fkerimk.com/scythe/icon.png" />

A personal game development framework or anything like that, idk.

## What is this?

A simple, mod based game development framework I made for my own games; containing basic tools and features.

Nothing fancy. It’s not designed to take you to space. You can take a look if you want, but it’s probably not a project you’ll need.

> [!NOTE]  
> Scythe is not a game engine. On the contrary, it’s developed with Unity. It’s simply a Unity-based toolkit/framework.

## 🛠 License

Scythe is licensed under the [LGPL-2.1 license](./LICENSE).

## How can use it?

> [!CAUTION] 
> Scythe hasn’t even learned to crawl yet. It’s not mature enough to be of any real use to you.

Make sure you have the .NET 10 SDK packages installed.

```bash
git clone --recurse-submodules https://github.com/fkerimk/scythe.git
cd ./scythe/
./build/build.sh
```

It automatically builds modules like lib, util, and run. After building lib, it will prompt you to build the core.

> To build the core module, open the core project with Unity 6000.2.6f2 (recommended), and create a build at:
> - Linux: `./core/Builds/linux-x64/scythe-core.x86_64`
> - Windows: `./core/Builds/win-x64/scythe-core.exe`

You can configure the build using `./build/config.ini`.

> [!CAUTION]  
> Support for the Windows is partially available, and almost everything is compatible.
However, the build script is outdated and has not been tested yet.
Do not take a build on Windows.

> [!NOTE]  
> Cross-platform builds can be made from both operating systems.
You can take a Windows build directly from Linux.
