using System.Collections.Generic;

namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class SettingsUpdateRequest
    {
        /// <summary>Optional list of the superuser allowed individual IPs and subnets (in CIDR notation).</summary>
        public List<string>? SuperuserIPs { get; set; }
        public SmtpSettingsUpdateRequest? Smtp { get; set; }
        public BackupSettingsUpdateRequest? Backups { get; set; }
        public S3SettingsUpdateRequest? S3 { get; set; }
        public MetaSettingsUpdateRequest? Meta { get; set; }
        public RateLimitSettingsUpdateRequest? RateLimits { get; set; }
        public TrustedProxySettingsUpdateRequest? TrustedProxy { get; set; }
        public BatchSettingsUpdateRequest? Batch { get; set; }
        public LogSettingsUpdateRequest? Logs { get; set; }
    }
}
