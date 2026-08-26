namespace PocketBase.Blazor.IntegrationTests.Clients.Logging;

using Blazor.Models;
using Blazor.Responses.Logging;

[Trait("Category", "Integration")]
[Collection("PocketBase.Blazor.Admin")]
public class GetOneTests
{
    private readonly IPocketBase _pb;

    public GetOneTests(PocketBaseAdminFixture fixture)
    {
        _pb = fixture.Client;
    }

    [Fact]
    public async Task GetOneAsync_ReturnsLog_WhenValidId()
    {
        // Arrange - get an existing log id (ensure at least one log exists - DeleteLogs may have cleared them)
        Result<ListResult<LogResponse>> listResult = await _pb.Log.GetListAsync(
            perPage: 1,
            options: new ListOptions()
            { 
                SkipTotal = true
            });
        listResult.IsSuccess.Should().BeTrue();
        for (int i = 0; i < 5 && !listResult.Value.Items.Any(); i++)
        {
            // Generate a request log by creating a temporary post (reads are not reliably logged in 0.40.1, writes are)
            Result<RecordModel> gen = await _pb.Collection("posts").CreateAsync<RecordModel>(new
            {
                title = $"loggen-{Guid.NewGuid():N}"[..12],
                slug = $"loggen-{Guid.NewGuid():N}"[..12],
                content = "log gen",
                author = "admin"
            });
            gen.IsSuccess.Should().BeTrue();
            await Task.Delay(2500);
            listResult = await _pb.Log.GetListAsync(perPage: 1, options: new ListOptions { SkipTotal = true });
            listResult.IsSuccess.Should().BeTrue();
        }
        listResult.Value.Items.Should().NotBeEmpty();
        string? logId = listResult.Value.Items.First().Id;

        // Act
        Result<LogResponse> result = await _pb.Log.GetOneAsync(logId!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(logId);
        result.Value.Message.Should().NotBeNullOrEmpty();
        result.Value.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOneAsync_ReturnsNotFound_WhenInvalidId()
    {
        // Arrange
        string invalidId = "non_existent_log_123";

        // Act
        Result<LogResponse> result = await _pb.Log.GetOneAsync(invalidId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors[0].Should().NotBeNull();
        result.Errors[0].Message.Should().Contain("404");
    }

    [Fact]
    public async Task GetOneAsync_ReturnsLogWithOptions_WhenFieldsSpecified()
    {
        // Arrange - ensure at least one log exists
        Result<ListResult<LogResponse>> listResult = await _pb.Log.GetListAsync(perPage: 1);
        listResult.IsSuccess.Should().BeTrue();
        for (int i = 0; i < 5 && !listResult.Value.Items.Any(); i++)
        {
            Result<RecordModel> gen = await _pb.Collection("posts").CreateAsync<RecordModel>(new
            {
                title = $"loggen-{Guid.NewGuid():N}"[..12],
                slug = $"loggen-{Guid.NewGuid():N}"[..12],
                content = "log gen",
                author = "admin"
            });
            gen.IsSuccess.Should().BeTrue();
            await Task.Delay(2500);
            listResult = await _pb.Log.GetListAsync(perPage: 1);
            listResult.IsSuccess.Should().BeTrue();
        }
        listResult.Value.Items.Should().NotBeEmpty();
        string? logId = listResult.Value.Items.First().Id;

        CommonOptions options = new CommonOptions
        {
            Fields = "id,message,level"
        };

        // Act
        Result<LogResponse> result = await _pb.Log.GetOneAsync(logId!, options);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(logId);
        result.Value.Message.Should().NotBeNullOrEmpty();
        // Note: When using fields filter, other properties might be null/default
    }

    [Fact]
    public async Task GetOneAsync_ThrowsArgumentException_WhenIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _pb.Log.GetOneAsync(""));
    }

    [Fact]
    public async Task GetOneAsync_ThrowsArgumentException_WhenIdIsWhitespace()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _pb.Log.GetOneAsync("   "));
    }

    [Fact]
    public async Task GetOneAsync_ThrowsArgumentNullException_WhenIdIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _pb.Log.GetOneAsync(null!));
    }
}
