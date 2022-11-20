
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "CriFs.V2.Hook/CriFs.V2.Hook.csproj" `
              -PackageName "CriFs.V2.Hook" `
			  -ReadmePath "README.md" `
			  @args

./Publish.ps1 -ProjectPath "Extensions/CriFs.V2.Hook.Awb/CriFs.V2.Hook.Awb.csproj" `
              -PackageName "CriFs.V2.Hook.Awb" `
			  -ReadmePath "README-AWB.md" `
			  -PublishOutputDir "Publish/ToUpload/AWB" `
			  @args

Pop-Location