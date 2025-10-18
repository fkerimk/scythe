# SCYTHE <img width="24" height="24" alt="icon" src="https://fkerimk.com/scythe/icon.png" />

A personal game development framework or anything like that, idk.

## What is this?

A simple, mod based game development framework I made for my own games; containing basic tools and features.

Nothing fancy. It’s not designed to take you to space. You can take a look if you want, but it’s probably not a project you’ll need.

> [!NOTE]  
> Scythe is not a game engine. It’s developed with Unity. It’s simply a Unity-based toolkit/framework.

## 🛠 License

Scythe is licensed under the [LGPL-2.1 license](./LICENSE).

## Building

> [!CAUTION] 
> Scythe hasn’t even learned to crawl yet. It’s not mature enough to be of any real use to you.

Make sure you have the .NET 10 SDK packages installed.

Builder is only available for the fish shell.

> [!NOTE] 
> Make sure you’ve entered the correct Unity path in `build/config.ini`. You can configure the build using `build/config.ini`.

```bash
git clone --recurse-submodules https://github.com/fkerimk/scythe.git
cd scythe
build/build.fish
```

It automatically builds lib, util, core, run and the mod.

> [!CAUTION]  
> Support for the Windows is no longer available.

> [!NOTE]  
> You can take a Windows build directly from Linux, you just can’t build from Windows.