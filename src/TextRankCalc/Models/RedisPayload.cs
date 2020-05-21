namespace TextRankCalc.Models
{
    public class RedisPayload
    {
        public string Description { get; set; }
        public string Data { get; set; }
        public double Rank { get; set; } = -1;
    }
}