namespace PocketBase.Blazor.IntegrationTests.Clients.Logging;

using Blazor.Models;
using Blazor.Responses.Logging;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class DeleteLogsTests
{
    private readonly IPocketBase _pb;

    public DeleteLogsTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_AsSuperuser()
    {
        // Arrange - ensure at least one request was logged
        Result<ListResult<LogResponse>> before = await _pb.Log.GetListAsync(perPage: 1);
        before.IsSuccess.Should().BeTrue();

        // Act
        Result result = await _pb.Log.DeleteAsync();

        // Assert - delete succeeds (204)
        result.IsSuccess.Should().BeTrue();

        // Verify subsequent list succeeds (may be empty or contain the delete request itself)
        Result<ListResult<LogResponse>> after = await _pb.Log.GetListAsync(perPage: 1);
        after.IsSuccess.Should().BeTrue();

        // Re-seed a log entry so subsequent tests (GetOne) that assume logs exist don't flake
        // (0.40.x persists request logs asynchronously, so we create a record and wait)
        Result<RecordModel> seed = await _pb.Collection("posts").CreateAsync<RecordModel>(new
        {
            title = $"logseed-{Guid.NewGuid():N}"[..12],
            slug = $"logseed-{Guid.NewGuid():N}"[..12],
            content = "log seed",
            author = "admin"
        });
        seed.IsSuccess.Should().BeTrue();
        await Task.Delay(3500);
    }
}
