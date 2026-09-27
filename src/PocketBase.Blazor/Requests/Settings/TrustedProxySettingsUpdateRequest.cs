using System.Collections.Generic;

namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class TrustedProxySettingsUpdateRequest
    {
        public List<string>? Headers { get; set; }
        public bool? UseLeftmostIP { get; set; }
    }
}
