namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class S3SettingsUpdateRequest
    {
        public bool? Enabled { get; set; }
        public string? Bucket { get; set; }
        public string? Region { get; set; }
        public string? Endpoint { get; set; }
        public string? AccessKey { get; set; }
        public string? Secret { get; set; }
        public bool? ForcePathStyle { get; set; }
    }
}
