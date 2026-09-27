using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PocketBase.Blazor.Http;
using PocketBase.Blazor.Options;
using PocketBase.Blazor.Responses.Auth;
using PocketBase.Blazor.Store;

namespace PocketBase.Blazor.Clients.Admin
{
    /// <inheritdoc />
    public class AdminsClient : IAdminsClient
    {
        private const string SuperusersCollection = "_superusers";
        private PocketBaseStore? _authStore;
        private readonly IHttpTransport _http;

        /// <inheritdoc />
        public AdminsClient(IHttpTransport http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> AuthWithPasswordAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email must be provided.", nameof(email));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must be provided.", nameof(password));

            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["identity"] = email,
                ["password"] = password
            };

            Result<AuthResponse> result = await _http.SendAsync<AuthResponse>(HttpMethod.Post, $"api/collections/{SuperusersCollection}/auth-with-password", body, cancellationToken: cancellationToken);

            if (result.IsSuccess)
            {
                _authStore?.Save(result.Value);
                return Result.Ok(result.Value);
            }
            else
            {
                return Result.Fail(result.Errors);
            }
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> AuthRefreshAsync(CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            options ??= new CommonOptions();
            options.Query = options.BuildQuery();

            Result<AuthResponse> result = await _http.SendAsync<AuthResponse>(HttpMethod.Post, $"api/collections/{SuperusersCollection}/auth-refresh", query: options.Query, cancellationToken: cancellationToken);

            if (result.IsSuccess)
            {
                _authStore?.Save(result.Value);
                return Result.Ok(result.Value);
            }
            else
            {
                return Result.Fail(result.Errors);
            }
        }

        /// <inheritdoc />
        public async Task<Result<AuthResponse>> ImpersonateAsync(string collectionName, string recordId, int duration, CommonOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(collectionName, nameof(collectionName));
            ArgumentException.ThrowIfNullOrWhiteSpace(recordId, nameof(recordId));

            Dictionary<string, object> body = new Dictionary<string, object>()
            {
                ["duration"] = duration,
            };

            options ??= new CommonOptions();
            options.Query = options.BuildQuery();

            Result<AuthResponse> result = await _http.SendAsync<AuthResponse>(HttpMethod.Post, $"api/collections/{collectionName}/impersonate/{recordId}", body, options.Query, cancellationToken: cancellationToken);

            if (result.IsSuccess)
            {
                _authStore?.Save(result.Value);
                return Result.Ok(result.Value);
            }
            else
            {
                return Result.Fail(result.Errors);
            }
        }

        /// <inheritdoc />
        public Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
        {
            // PocketBase has no logout endpoint; superuser sessions are stateless JWTs,
            // so logging out only requires clearing the local auth state.
            _authStore?.Clear();
            return Task.FromResult(Result.Ok());
        }

        /// <inheritdoc />
        public void SetStore(PocketBaseStore store)
        {
            _authStore = store ?? throw new ArgumentNullException(nameof(store));
        }
    }
}
