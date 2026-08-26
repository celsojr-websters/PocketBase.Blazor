namespace PocketBase.Blazor.Responses.Settings
{
    public sealed class LogSettingsResponse
    {
        public long MaxDataSize { get; init; }
        public int MaxDays { get; init; }
        public int MinLevel { get; init; }
        public bool LogIp { get; init; }
        public bool LogAuthId { get; init; }
    }
}
