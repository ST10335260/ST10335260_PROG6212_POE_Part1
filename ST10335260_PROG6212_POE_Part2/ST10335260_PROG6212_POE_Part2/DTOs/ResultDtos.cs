using System.ComponentModel.DataAnnotations;

namespace ST10335260_PROG6212_POE_Part2.DTOs;

public class ResultRequestDto
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int Position { get; set; }

    [Range(1, int.MaxValue)]
    public int? OverallRank { get; set; }

    [Range(1, int.MaxValue)]
    public int? CategoryRank { get; set; }

    [Range(0, 100)]
    public decimal? PacePerKm { get; set; }

    public bool Verified { get; set; }
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int? BibNumber { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public int OverallRank { get; set; }
    public int? CategoryRank { get; set; }
    public decimal? PacePerKm { get; set; }
    public bool Verified { get; set; }
}

public class MyResultDto
{
    public int ResultId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public int OverallRank { get; set; }
    public int TotalFinishers { get; set; }
    public decimal? PacePerKm { get; set; }
    public int? BibNumber { get; set; }
}
