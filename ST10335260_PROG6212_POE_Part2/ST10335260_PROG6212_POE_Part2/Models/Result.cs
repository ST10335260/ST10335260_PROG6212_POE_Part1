namespace ST10335260_PROG6212_POE_Part2.Models;

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public Enrolment? Enrolment { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int? Position { get; set; }
    public int? OverallRank { get; set; }
    public int? CategoryRank { get; set; }
    public decimal? PacePerKm { get; set; }
    public bool Verified { get; set; } = false;
}