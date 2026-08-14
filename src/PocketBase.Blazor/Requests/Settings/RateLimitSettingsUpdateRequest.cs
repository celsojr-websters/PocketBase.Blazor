using System.Collections.Generic;

namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class RateLimitSettingsUpdateRequest
    {
        public List<RateLimitRuleUpdateRequest>? Rules { get; set; }
        public List<string>? ExcludedIPs { get; set; }
        public bool? Enabled { get; set; }
    }
}
