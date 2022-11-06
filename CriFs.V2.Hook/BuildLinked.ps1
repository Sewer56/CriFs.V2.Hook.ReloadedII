# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/CriFs.V2.Hook/*" -Force -Recurse
dotnet publish "./CriFs.V2.Hook.csproj" -c Release -o "$env:RELOADEDIIMODS/CriFs.V2.Hook" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location