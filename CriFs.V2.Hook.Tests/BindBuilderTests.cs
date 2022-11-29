using CriFs.V2.Hook.Bind;
using CriFs.V2.Hook.Bind.Utilities;
using CriFs.V2.Hook.Interfaces;
using static CriFs.V2.Hook.Tests.Utilities;

namespace CriFs.V2.Hook.Tests;

public class BindBuilderTests
{
    [Fact]
    public void Build_BaseLine()
    {
        // Arrange
        using var tempFolder = new TemporaryFolderAllocation(Assets.TempFolder);
        var builder = new BindBuilder(tempFolder.FolderPath);
        builder.AddItem(new BuilderItem("", Assets.ButtonPromptsMod1Cpk, GetFilesInDirectory(Assets.ButtonPromptsMod1Cpk)));
        builder.AddItem(new BuilderItem("", Assets.ButtonPromptsMod2Cpk, GetFilesInDirectory(Assets.ButtonPromptsMod2Cpk)));

        // Act
        var outputDir = builder.Build(new List<Action<ICriFsRedirectorApi.BindContext>>());
        
        // Assert
        Assert.Equal(2, GetFilesInDirectory(outputDir).Count);
    }
}