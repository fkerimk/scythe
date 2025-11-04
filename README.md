# SCYTHE

Just a game engine. The spiritual successor to Scythe-Unity, a Unity based game development framework (unreleased).

## What is this?

A simple engine designed for developing easily editable and moddable games at a basic level.

Nothing fancy. It’s not designed to take you to space. You can take a look if you want, but it’s probably not a tool you’ll need.

## 🛠 License

Scythe is licensed under the [LGPL-2.1 license](./LICENSE).

## Building

> [!CAUTION]  
> Scythe hasn’t even learned to crawl yet. It’s not mature enough to be of any real use to you.

> [!CAUTION]  
> When you build the project, some essential resources required to run it are still missing from the repository. It is recommended for experimental or educational use only.

Make sure you have the .NET 10 SDK packages installed.

```bash
git clone https://github.com/fkerimk/scythe.git
cd scythe
dotnet build
```

For potential build errors *(especially on other platforms)*, you can try removing the `<PublishAot>true</PublishAot>` setting in `scythe.csproj`.
