using System.Collections.Generic;

namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class SettingsResponse
    {
        /// <summary>List of the superuser allowed individual IPs and subnets (in CIDR notation).</summary>
        public List<string> SuperuserIPs { get; init; } = [];
        public SmtpSettingsResponse? Smtp { get; init; }
        public BackupSettingsResponse? Backups { get; init; }
        public S3SettingsResponse? S3 { get; init; }
        public MetaSettingsResponse? Meta { get; init; }
        public RateLimitSettingsResponse? RateLimits { get; init; }
        public TrustedProxySettingsResponse? TrustedProxy { get; init; }
        public BatchSettingsResponse? Batch { get; init; }
        public LogSettingsResponse? Logs { get; init; }
    }
}
