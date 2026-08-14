namespace PocketBase.Blazor.Requests.Settings
{
    public sealed class RateLimitRuleUpdateRequest
    {
        public string? Label { get; set; }
        public string? Audience { get; set; }
        public int? Duration { get; set; }
        public int? MaxRequests { get; set; }
    }
}
