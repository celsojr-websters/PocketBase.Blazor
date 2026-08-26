namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class LogSettingsUpdateRequest
    {
        public long? MaxDataSize { get; set; }
        public int? MaxDays { get; set; }
        public int? MinLevel { get; set; }
        public bool? LogIP { get; set; }
        public bool? LogAuthId { get; set; }
    }
}
