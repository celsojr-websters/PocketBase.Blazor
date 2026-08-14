using System.Text.Json.Serialization;

namespace PocketBase.Blazor.Requests.Auth
{
    public class AuthWithOAuth2Request
    {
        /// <summary>The name of the OAuth2 client provider (eg. "google").</summary>
        public required string Provider { get; init; }

        /// <summary>The authorization code returned from the initial request.</summary>
        public required string Code { get; init; }

        /// <summary>
        /// The PKCE code verifier sent as part of the code_challenge with the initial request.
        /// Only required when the provider uses PKCE.
        /// </summary>
        public string? CodeVerifier { get; init; }

        /// <summary>The redirect url sent with the initial request.</summary>
        [JsonPropertyName("redirectURL")]
        public string? RedirectUrl { get; init; }

        /// <summary>Optional data used when creating a new auth record.</summary>
        public object? CreateData { get; init; }
    }
}
