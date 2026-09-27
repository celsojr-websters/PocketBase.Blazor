namespace PocketBase.Blazor.UnitTests.Domain.Collections;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Collections;
using Blazor.Http;
using Blazor.Models;
using Blazor.Options;
using FluentAssertions;
using FluentResults;

[Trait("Category", "Unit")]
public class CollectionClientBodyMergeTests
{
    [Fact]
    public async Task UpdateAsync_WithOnlyAnonymousOptionsBody_ShouldSendAnonymousBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        CollectionClient client = new CollectionClient(transport);

        // Act - mirrors the OTP enable flow that used to drop the anonymous body
        Result<CollectionModel> result = await client.UpdateAsync<CollectionModel>(
            "users",
            options: new CommonOptions
            {
                Body = new
                {
                    otp = new
                    {
                        enabled = true
                    }
                }
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Method.Should().Be(HttpMethod.Patch);
        string json = JsonSerializer.Serialize(transport.Requests[0].Body, new PocketBaseOptions().JsonSerializerOptions);
        JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("otp", out JsonElement otp).Should().BeTrue();
        otp.GetProperty("enabled").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WithAnonymousOptionsBody_ShouldMergeWithMainBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        CollectionClient client = new CollectionClient(transport);

        // Act
        Result<CollectionModel> result = await client.UpdateAsync<CollectionModel>(
            "users",
            new Dictionary<string, object?>
            {
                ["name"] = "renamed"
            },
            options: new CommonOptions
            {
                Body = new
                {
                    description = "updated description"
                }
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Body.Should().BeOfType<Dictionary<string, object?>>();
        Dictionary<string, object?> merged = (Dictionary<string, object?>)transport.Requests[0].Body!;
        merged.Should().ContainKey("name").WhoseValue.Should().Be("renamed");
        merged.Should().ContainKey("description").WhoseValue.Should().Be("updated description");
    }

    [Fact]
    public async Task UpdateAsync_MainBodyShouldOverrideOptionsBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        CollectionClient client = new CollectionClient(transport);

        // Act
        Result<CollectionModel> result = await client.UpdateAsync<CollectionModel>(
            "users",
            new Dictionary<string, object?>
            {
                ["name"] = "from-main-body"
            },
            options: new CommonOptions
            {
                Body = new Dictionary<string, object?>
                {
                    ["name"] = "from-options-body"
                }
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        Dictionary<string, object?> merged = (Dictionary<string, object?>)transport.Requests[0].Body!;
        merged["name"].Should().Be("from-main-body");
    }

    [Fact]
    public async Task CreateAsync_WithOnlyAnonymousOptionsBody_ShouldSendAnonymousBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        CollectionClient client = new CollectionClient(transport);

        // Act
        Result<CollectionModel> result = await client.CreateAsync<CollectionModel>(
            options: new CommonOptions
            {
                Body = new
                {
                    name = "posts",
                    type = "base"
                }
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Method.Should().Be(HttpMethod.Post);
        string json = JsonSerializer.Serialize(transport.Requests[0].Body, new PocketBaseOptions().JsonSerializerOptions);
        JsonDocument doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("name").GetString().Should().Be("posts");
        doc.RootElement.GetProperty("type").GetString().Should().Be("base");
    }

    [Fact]
    public async Task UpdateAsync_WithOnlyAnonymousOptionsBodyInDictionary_ShouldStillSendBody()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        CollectionClient client = new CollectionClient(transport);

        // Act
        Result<CollectionModel> result = await client.UpdateAsync<CollectionModel>(
            "users",
            options: new CommonOptions
            {
                Body = new Dictionary<string, object?>
                {
                    ["status"] = "active"
                }
            });

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Body.Should().BeOfType<Dictionary<string, object?>>();
        Dictionary<string, object?> body = (Dictionary<string, object?>)transport.Requests[0].Body!;
        body["status"].Should().Be("active");
    }

    private sealed class FakeHttpTransport : IHttpTransport
    {
        public List<(HttpMethod Method, string Path, object? Body)> Requests { get; } = [];

        public string BaseUrl => "http://localhost";

        public string BuildUrl(string endpoint) => $"{BaseUrl}/{endpoint.TrimStart('/')}";

        public Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            Requests.Add((method, path, body));

            if (typeof(T) == typeof(CollectionModel))
            {
                return Task.FromResult((Result<T>)(object)Result.Ok(new CollectionModel()));
            }

            throw new NotSupportedException($"Unexpected request in test double: {method} {path} ({typeof(T).Name})");
        }

        public Task<Result> SendAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result> SendAsync(HttpMethod method, string path, MultipartFile file,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<Stream>> SendForStreamAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Result<byte[]>> SendForBytesAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<string> SendForSseAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
