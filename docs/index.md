# About

CRI FileSystem V2 Hook is a mod based on [Reloaded II](https://reloaded-project.github.io/Reloaded-II/) that allows you to add/replace files in games that use CRI Middleware and the CPK container.  

## Support

!!! danger "Fuck Denuvo"

### x64 Games

| Game | Build Date | CRI FS Version | Compiler | Notes | 
|------|------------|----------------|----------|-------|
|      |            |                |          |       | 

### x86 Games

| Game | Build Date | CRI FS Version | Compiler | Notes | 
|------|------------|----------------|----------|-------|
|      |            |                |          |       | 

### AArch64 (ARM64) Games

!!! info "Maybe someday 😉."

## Additional Features

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