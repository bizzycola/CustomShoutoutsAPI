namespace CustomShoutoutsAPI.GraphQL.Results
{
    public class SiteStatsResult
    {
        public int UserCount { get; set; }
        public int CustomShoutoutCount { get; set; }
        public long TotalShoutoutCalls { get; set; }
    }
}
