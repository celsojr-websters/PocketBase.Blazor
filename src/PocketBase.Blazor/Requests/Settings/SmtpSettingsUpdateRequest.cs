namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class SmtpSettingsUpdateRequest
    {
        public bool? Enabled { get; set; }
        public int? Port { get; set; }
        public string? Host { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? AuthMethod { get; set; }
        public bool? Tls { get; set; }
        public string? LocalName { get; set; }
    }
}
