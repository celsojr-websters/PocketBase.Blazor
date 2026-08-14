namespace PocketBase.Blazor.UnitTests.Domain.Admin;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Clients.Admin;
using Blazor.Clients.Realtime;
using Blazor.Events;
using Blazor.Http;
using Blazor.Options;
using Blazor.Responses.Auth;
using Blazor.Store;
using FluentAssertions;
using FluentResults;

[Trait("Category", "Unit")]
public class AdminsClientTests
{
    [Fact]
    public async Task AuthWithPasswordAsync_ShouldPostToSuperusersAuthWithPasswordEndpoint()
    {
        // Arrange
        CapturingHttpTransport transport = new CapturingHttpTransport();
        PocketBaseStore store = CreateStore();
        AdminsClient client = new AdminsClient(transport);
        client.SetStore(store);

        // Act
        Result<AuthResponse> result = await client.AuthWithPasswordAsync("admin@example.com", "secret");

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Method.Should().Be(HttpMethod.Post);
        transport.Requests[0].Path.Should().Be("api/collections/_superusers/auth-with-password");

        Dictionary<string, object>? body = transport.Requests[0].Body as Dictionary<string, object>;
        body.Should().NotBeNull();
        body!["identity"].Should().Be("admin@example.com");
        body["password"].Should().Be("secret");

        store.Token.Should().Be("admin-token");
    }

    [Fact]
    public async Task AuthWithPasswordAsync_WhenTransportFails_ShouldReturnFailureWithoutSavingStore()
    {
        // Arrange
        CapturingHttpTransport transport = new CapturingHttpTransport
        {
            AuthResult = Result.Fail<AuthResponse>("invalid credentials")
        };
        PocketBaseStore store = CreateStore();
        AdminsClient client = new AdminsClient(transport);
        client.SetStore(store);

        // Act
        Result<AuthResponse> result = await client.AuthWithPasswordAsync("admin@example.com", "wrong");

        // Assert
        result.IsSuccess.Should().BeFalse();
        store.Token.Should().BeNull();
    }

    [Fact]
    public async Task AuthRefreshAsync_ShouldPostToSuperusersAuthRefreshEndpoint()
    {
        // Arrange
        CapturingHttpTransport transport = new CapturingHttpTransport();
        PocketBaseStore store = CreateStore();
        AdminsClient client = new AdminsClient(transport);
        client.SetStore(store);

        // Act
        Result<AuthResponse> result = await client.AuthRefreshAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Method.Should().Be(HttpMethod.Post);
        transport.Requests[0].Path.Should().Be("api/collections/_superusers/auth-refresh");
        store.Token.Should().Be("admin-token");
    }

    [Fact]
    public async Task ImpersonateAsync_ShouldPostToCollectionImpersonateEndpoint()
    {
        // Arrange
        CapturingHttpTransport transport = new CapturingHttpTransport();
        PocketBaseStore store = CreateStore();
        AdminsClient client = new AdminsClient(transport);
        client.SetStore(store);

        // Act
        Result<AuthResponse> result = await client.ImpersonateAsync("users", "rec_123", 3600);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transport.Requests.Should().ContainSingle();
        transport.Requests[0].Path.Should().Be("api/collections/users/impersonate/rec_123");
        transport.Requests[0].Body.Should().BeAssignableTo<IDictionary<string, object>>();
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearStoreWithoutMakingHttpCall()
    {
        // Arrange
        CapturingHttpTransport transport = new CapturingHttpTransport();
        PocketBaseStore store = CreateStore();
        AdminsClient client = new AdminsClient(transport);
        client.SetStore(store);

        await client.AuthWithPasswordAsync("admin@example.com", "secret");
        store.Token.Should().NotBeNull();

        // Act
        Result result = await client.LogoutAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        store.Token.Should().BeNull();
        store.CurrentSession.Should().BeNull();

        // PocketBase has no server-side logout endpoint - only the initial
        // auth call should have hit the transport.
        transport.Requests.Should().ContainSingle();
    }

    private static PocketBaseStore CreateStore()
    {
        return new PocketBaseStore(new AuthStore(), new NoopRealtimeClient(), new NoopRealtimeStreamClient());
    }

    private sealed class CapturingHttpTransport : IHttpTransport
    {
        public List<(HttpMethod Method, string Path, object? Body, IDictionary<string, object?>? Query)> Requests { get; } = [];

        public Result<AuthResponse> AuthResult { get; init; } = Result.Ok(new AuthResponse { Token = "admin-token" });

        public string BaseUrl => "http://localhost";

        public string BuildUrl(string endpoint) => $"{BaseUrl}/{endpoint.TrimStart('/')}";

        public Task<Result<T>> SendAsync<T>(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            Requests.Add((method, path, body, query));

            if (typeof(T) == typeof(AuthResponse))
            {
                return Task.FromResult((Result<T>)(object)AuthResult);
            }

            throw new NotSupportedException($"Unexpected request in test double: {method} {path} ({typeof(T).Name})");
        }

        public Task<Result> SendAsync(HttpMethod method, string path, object? body = null,
            IDictionary<string, object?>? query = null, CancellationToken cancellationToken = default)
        {
            Requests.Add((method, path, body, query));
            return Task.FromResult(Result.Ok());
        }

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
