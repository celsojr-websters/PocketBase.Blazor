namespace PocketBase.Blazor.UnitTests.Domain.Hosting;

using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Crons;
using Blazor.Hosting.Services;
using Blazor.Models;
using Blazor.Options;
using FluentAssertions;

[Trait("Category", "Unit")]
[Trait("Requires", "FileSystem")]
public class PocketBaseVersionTests
{
    [Fact]
    public void BinaryResolverVersion_ShouldBeValidSemver()
    {
        // Arrange & Act
        string version = PocketBaseBinaryResolver.Version;

        // Assert
        Regex.IsMatch(version, @"^\d+\.\d+\.\d+$").Should().BeTrue($"'{version}' is not a valid semver");
    }

    [Fact]
    public async Task BinaryResolverVersion_ShouldMatchCronGeneratorGoModuleVersion()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), "pb_crons_test_version");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

        CronGenerator generator = new CronGenerator();
        CronManifest manifest = new CronManifest { Crons = [new() { Id = "hello", HandlerBody = "" }] };
        CronGenerationOptions options = new CronGenerationOptions
        {
            ProjectDirectory = tempDir,
            BuildBinary = false
        };

        await generator.GenerateAsync(manifest, options, CancellationToken.None);

        // Act
        string goMod = await File.ReadAllTextAsync(Path.Combine(tempDir, "go.mod"));
        Match match = Regex.Match(goMod, @"require github\.com/pocketbase/pocketbase v(\d+\.\d+\.\d+)");

        // Assert
        match.Success.Should().BeTrue("go.mod should pin a PocketBase version");
        match.Groups[1].Value.Should().Be(PocketBaseBinaryResolver.Version,
            "the hosted binary and generated Go module should target the same PocketBase release");
    }
}
