namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class LogSettingsUpdateRequest
    {
        public int? MaxDays { get; set; }
        public int? MinLevel { get; set; }
        public bool? LogIP { get; set; }
        public bool? LogAuthId { get; set; }
    }
}
