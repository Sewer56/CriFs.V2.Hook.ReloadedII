# About

CRI FileSystem V2 Hook is a mod based on [Reloaded II](https://reloaded-project.github.io/Reloaded-II/) that allows you to add/replace files in games that use CRI Middleware and the CPK container.

# Support

This universal mod targets games using various versions of CRI MiddleWare's V2 File System.

For a full list of supported games, see the [wiki support table](https://sewer56.dev/CriFs.V2.Hook.ReloadedII/#support).

## Additional Features

- Mods can use case-insensitive file paths. (Note: CRI is Case Sensitive)  
- Supports games using both UTF-8 and ANSI encoding.  
- Hot Reload (Add/Replace files without app restart).  
- API (dynamically add files, etc.)  
- Log files accessed by application.  
- Log redirected files.  
- Replace music in AWB files inside CPKs without using additional disk space, [via an extension mod](./usage-awb.md).  

# Building

- Install [Reloaded II](https://github.com/Reloaded-Project/Reloaded-II/releases/latest).  
- Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).  
- [Optional] Download Visual Studio/Rider and open the .sln file.  

When you build your project, the files will automatically be copied to the right directory and be loaded by Reloaded.  
[Refer to the Reloaded wiki if you need more information](https://reloaded-project.github.io/Reloaded-II/DevelopmentEnvironmentSetup/).