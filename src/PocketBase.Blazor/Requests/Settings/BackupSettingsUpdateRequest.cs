namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class BackupSettingsUpdateRequest
    {
        public string? Cron { get; set; }
        public int? CronMaxKeep { get; set; }
        public S3SettingsUpdateRequest? S3 { get; set; }
    }
}
