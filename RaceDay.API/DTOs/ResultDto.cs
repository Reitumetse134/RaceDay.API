namespace RaceDay.API.DTOs
{
    public class ResultDto
    {
        public TimeSpan FinishTime { get; set; }
        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }
        public string Pace { get; set; } = string.Empty;
    }
}