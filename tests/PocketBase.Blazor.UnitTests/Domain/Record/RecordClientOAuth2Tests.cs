namespace PocketBase.Blazor.UnitTests.Domain.Record;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Record;
using Blazor.Clients.Realtime;
using Blazor.Events;
using Blazor.Http;
using Blazor.Options;
using Blazor.Requests.Auth;
using Blazor.Responses.Auth;
using Blazor.Store;
using Blazor.UnitTests.TestHelpers.Extensions;
using FluentAssertions;
using FluentResults;

[Trait("Category", "Unit")]
public class RecordClientOAuth2Tests
{
    [Fact]
    public async Task AuthWithOAuth2CodeAsync_ShouldPostToCollectionOAuth2EndpointAndSaveStore()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport();
        PocketBaseStore store = CreateStore();
        RecordClient client = new RecordClient("users", transport, store);

        // Act
        Result<AuthRecordResponse> result = await client.AuthWithOAuth2CodeAsync(new AuthWithOAuth2Request
        {
            Provider = "github",
            Code = "auth-code".ToBase64(),
            CodeVerifier = "verifier".ToBase64(),
            RedirectUrl = "http://localhost/callback",
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Path.Should().Be("api/collections/users/auth-with-oauth2");
        store.Token.Should().Be("oauth2-token".ToBase64());
        store.CurrentSession.Should().BeSameAs(result.Value);
    }

    [Fact]
    public async Task AuthWithOAuth2CodeAsync_WhenTransportFails_ShouldReturnFailureWithoutSavingStore()
    {
        // Arrange
        FakeHttpTransport transport = new FakeHttpTransport
        {
            AuthResult = Result.Fail<AuthRecordResponse>("provider rejected code")
        };
        PocketBaseStore store = CreateStore();
        RecordClient client = new RecordClient("users", transport, store);

        // Act
        Result<AuthRecordResponse> result = await client.AuthWithOAuth2CodeAsync(new AuthWithOAuth2Request
        {
            Provider = "github",
            Code = "bad-code".ToBase64(),
        });

        // Assert
        result.IsSuccess.Should().BeFalse();
        store.Token.Should().BeNull();
    }

    [Fact]
    public void AuthWithOAuth2Request_ShouldSerializeWithPocketBaseFieldNames()
    {
        // Arrange
        AuthWithOAuth2Request request = new AuthWithOAuth2Request
        {
            Provider = "github",
            Code = "auth-code".ToBase64(),
            CodeVerifier = "verifier".ToBase64(),
            RedirectUrl = "http://localhost/callback",
            CreateData = new Dictionary<string, object> { ["name"] = "Celso" },
        };

        // Act
        string json = JsonSerializer.Serialize(request, new PocketBaseOptions().JsonSerializerOptions);
        JsonDocument doc = JsonDocument.Parse(json);

        // Assert
        doc.RootElement.TryGetProperty("provider", out JsonElement provider).Should().BeTrue();
        provider.GetString().Should().Be("github");

        doc.RootElement.TryGetProperty("code", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("codeVerifier", out JsonElement codeVerifier).Should().BeTrue();
        codeVerifier.GetString().Should().Be("verifier".ToBase64());

        // PocketBase expects "redirectURL" (lowercase r, uppercase URL)
        doc.RootElement.TryGetProperty("redirectURL", out JsonElement redirectUrl).Should().BeTrue();
        redirectUrl.GetString().Should().Be("http://localhost/callback");

        doc.RootElement.TryGetProperty("createData", out _).Should().BeTrue();
    }

    [Fact]
    public void AuthWithOAuth2Request_ShouldSerializeNullsForOptionalFields()
    {
        // Arrange
        AuthWithOAuth2Request request = new AuthWithOAuth2Request
        {
            Provider = "google",
            Code = "auth-code".ToBase64(),
        };

        // Act
        string json = JsonSerializer.Serialize(request, new PocketBaseOptions().JsonSerializerOptions);
        JsonDocument doc = JsonDocument.Parse(json);

        // Assert - optional fields are present but null, which PocketBase ignores
        doc.RootElement.TryGetProperty("codeVerifier", out JsonElement codeVerifier).Should().BeTrue();
        codeVerifier.ValueKind.Should().Be(JsonValueKind.Null);
        doc.RootElement.TryGetProperty("redirectURL", out JsonElement redirectUrl).Should().BeTrue();
        redirectUrl.ValueKind.Should().Be(JsonValueKind.Null);
    }

    private static PocketBaseStore CreateStore()
    {
        return new PocketBaseStore(new AuthStore(), new NoopRealtimeClient(), new NoopRealtimeStreamClient());
    }

    private sealed class FakeHttpTransport : IHttpTransport
    {
        public List<(HttpMethod Method, string Path, object? Body)> Requests { get; } = [];

        public Result<AuthRecordResponse> AuthResult { get; init; } = Result.Ok(new AuthRecordResponse
        {
            Token = "oauth2-token".ToBase64(),
            Meta = new AuthMetaResponse { IsNew = false },
        });

        public string BaseUrl => "http://localhost";

        public string BuildUrl(string endpoint) => $"{BaseUrl}/{endpoint.TrimStart('/')}";

        public Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            Requests.Add((method, path, body));

            if (typeof(T) == typeof(AuthRecordResponse))
            {
                return Task.FromResult((Result<T>)(object)AuthResult);
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

    private sealed class NoopRealtimeClient : IRealtimeClient
    {
        public bool IsConnected => false;

        public event Action<IReadOnlyList<string>> OnDisconnect
        {
            add { }
            remove { }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task<IDisposable> SubscribeAsync(string collection, string recordId, Action<RealtimeRecordEvent> onEvent,
            CommonOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class NoopRealtimeStreamClient : IRealtimeStreamClient
    {
        public bool IsConnected => false;

        public event Action<IReadOnlyList<string>> OnDisconnect
        {
            add { }
            remove { }
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public IAsyncEnumerable<RealtimeRecordEvent> SubscribeAsync(string collection, string recordId,
            CommonOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UnsubscribeAsync(string collection, string? recordId = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
