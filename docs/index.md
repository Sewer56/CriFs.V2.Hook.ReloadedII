# About

CRI FileSystem V2 Hook is a mod based on [Reloaded II](https://reloaded-project.github.io/Reloaded-II/) that allows you to add/replace files in games that use CRI Middleware and the CPK container.  

## Support

!!! info "Listed in ascending order by Build Date."

!!! note "Games marked with a ⚠️ should run correctly but were not tested with existing mods."

!!! note "Games marked with a ‼️ I do not own and were untested. They may or may not work."

If you need to support an additional game, and it is not listed here; [consider contributing](adding-game-support.md).

### x64 Games

| Game                          | Build Date           | CRI FS Version | Compiler            | Notes                                                                                    | 
|-------------------------------|----------------------|----------------|---------------------|------------------------------------------------------------------------------------------|
| Yakuza Kiwami                 | Oct 6 2015 19:45:41  | 2.71.02        | MSC17.00.61030.0,MT | ⚠️ Loads files by ID, which CriFsV2Hook does not support hooking. Missing `SetPriority`. |
| One Piece: Burning Blood      | Oct 8 2015 13:15:23  | 2.70.00        | MSC16.00.40219.1,MT | Missing `SetPriority` function.                                                          |
| Tekken 7                      | Jul 27 2017 11:01:21 | 2.73.00        | MSC17.00.61030.0,MT | Game does not use CPK files. But it'll work if the game ever loads one.                  |
| Sonic Forces                  | Oct 6 2017 14:17:55  | 2.75.05        | MSC17.00.61030.0,MT | ⚠️                                                                                       | 
| Yakuza Kiwami 2               | Oct 6 2017 14:17:55  | 2.75.05        | MSC17.00.61030.0,MT | Game does not use CPK files. But it'll work if the game ever loads one.                  |
| Valkyria Chronicles 4         | Oct 6 2017 14:17:55  | 2.75.05        | MSC17.00.61030.0,MT |                                                                                          |
| Persona 5 Royal               | Dec 28 2021 11:03:45 | 2.81.6         | MSC19.00.24210.0,MT |                                                                                          | 
| Persona 3 Portable            | May 12 2022 19:34:26 | 2.82.15        | MSC19.16.27045.0,MT |                                                                                          |
| Persona 4 The Golden (64-bit) | May 12 2022 19:34:26 | 2.82.15        | MSC19.16.27045.0,MT |                                                                                          |

### x86 Games

!!! note "Bayonetta having lower CRI version despite older build date is NOT a typo."

| Game                           | Build Date           | CRI FS Version | Compiler   | Notes                           | 
|--------------------------------|----------------------|----------------|------------|---------------------------------|
| Sonic Generations              | Apr  1 2011 21:08:31 | 2.24.04        | MSC1500,MT |                                 | 
| Sonic Lost World               | Feb 19 2013 12:43:50 | 2.59.21        | MSC1600,MT | ⚠️                              | 
| Tales of Symphonia             | Apr 11 2013 12:11:23 | 2.60.00        | MSC1600,MT | Missing `SetPriority` function. | 
| One Piece: Unlimited World Red | Mar  3 2014 14:59:30 | 2.63.08        | MSC1600,MT | Missing `SetPriority` function. | 
| Bayonetta                      | Jan 27 2017 19:10:26 | 2.61.09        | MSC1600,MT | Missing `SetPriority` function. | 

### Supported But Recognized Games

!!! note "Games which are 'supported' but miss code necessary for this mod to work. (Needed code for this mod was unused and removed)"

| Game                          | Build Date           | CRI FS Version | Compiler            | Notes                                                                                | 
|-------------------------------|----------------------|----------------|---------------------|--------------------------------------------------------------------------------------|
| Sonic 4 Episode 1             | Apr  1 2011 21:08:31 | 2.24.04        | MSC1500,MT          | Does not Bind CPKs, uses them through internal API so methods missing.               |
| Sonic 4 Episode 2             | Apr  1 2011 21:08:31 | 2.24.04        | MSC1500,MT          | Same library as Sonic Generations; might be possible to transplant the missing code. | 
| NieR Automata (YoRHa Edition) | Jun 20 2016 18:17:30 | 2.73.00        | MSC17.00.61030.0,MT | Missing 'BindFiles'.                                                                 | 

!!! info "They would be supported if I ever took the time out to make a 'CPK Emulator'."

### AArch64 (ARM64) Games

!!! info "If [Reloaded3](https://reloaded-project.github.io/Reloaded-III/) one day arrives on the Switch, this mod will be expanded to support the Switch and ARM64 games."

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