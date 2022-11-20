# About

CRI FileSystem V2 Hook is a mod based on [Reloaded II](https://reloaded-project.github.io/Reloaded-II/) that allows you to add/replace files in games that use CRI Middleware and the CPK container.  

# Support

This universal mod targets x64 versions of games using CRI MiddleWare's V2 File System built with MSVC compiler.  
Work is based on `CRI File System/PCx64 Ver.2.81.6 Build:Dec 28 2021 11:03:45`.  
Other versions close to this version will likely work, but there's no guarantee.  

Currently it is known to work on the following applications.

- Persona 5 Royal  
- Persona 4 Golden Remaster Edition (2022, a.k.a. P4G64)  
- Sonic Frontiers

## Additional Features

- Hot Reload (Add/Replace files without app restart).  
- API (dynamically add files, set probing paths, etc.)  
- Log files accessed by application.  
- Replace music in AWB files inside CPKs without using additional disk space, [via an extension mod](./usage-awb.md).

# Building

- Install [Reloaded II](https://github.com/Reloaded-Project/Reloaded-II/releases/latest).  
- Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).  
- [Optional] Download Visual Studio/Rider and open the .sln file.  

When you build your project, the files will automatically be copied to the right directory and be loaded by Reloaded.  
[Refer to the Reloaded wiki if you need more information](https://reloaded-project.github.io/Reloaded-II/DevelopmentEnvironmentSetup/).