<div align="center">
	<h1>CRI FileSystem V2 Hook</h1>
	<img src="./docs/images/icon.png" Width="200" /><br/>
    <p>Mod to enable file replacement in more recent CRI games.</p>
</div>

# About

CRI FileSystem V2 hook is a mod based on [Reloaded II](https://reloaded-project.github.io/Reloaded-II/) that allows you to add/replace files in games that use CRI Middleware and the CPK container.  

# Basic Usage

[Read here.](./docs/usage.md)

# Support

This universal mod targets x64 versions of games using CRI MiddleWare's V2 File System built with MSVC compiler.  
Work is based on `CRI File System/PCx64 Ver.2.81.6 Build:Dec 28 2021 11:03:45`.  
Other versions close to this version will likely work, but there's no guarantee.  

Currently it is known to work on the following applications.

- Persona 5 Royal  
- Persona 4 Golden Remaster Edition (2022, a.k.a. P4G64)  

## Additonal Features

- Hot Reload (Add/Replace files without app restart).  
- API (dynamically add files, set probing paths, etc.)  
- Log files accessed by application.  

# Building

- Install [Reloaded II](https://github.com/Reloaded-Project/Reloaded-II/releases/latest).  
- Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).  
- [Optional] Download Visual Studio/Rider and open the .sln file.  

When you build your project, the files will automatically be copied to the right directory and be loaded by Reloaded.  
[Refer to the Reloaded wiki if you need more information](https://reloaded-project.github.io/Reloaded-II/DevelopmentEnvironmentSetup/).