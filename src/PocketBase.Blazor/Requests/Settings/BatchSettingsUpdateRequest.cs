namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class BatchSettingsUpdateRequest
    {
        public bool? Enabled { get; set; }
        public int? MaxRequests { get; set; }
        public int? Timeout { get; set; }
        public int? MaxBodySize { get; set; }
    }
}
