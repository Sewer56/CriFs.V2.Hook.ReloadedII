# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/crifs.v2.hook.awb/*" -Force -Recurse
dotnet publish "./CriFs.V2.Hook.Awb.csproj" -c Release -o "$env:RELOADEDIIMODS/crifs.v2.hook.awb" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location