using ST10335260_PROG6212_POE_Part2.DTOs;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Services;

public static class EnrolmentMapper
{
    public static string FullName(User? user) =>
        user is null ? string.Empty : $"{user.FirstName} {user.LastName}".Trim();

    public static EnrolmentDto ToDto(Enrolment e) => new()
    {
        EnrolmentId = e.EnrolmentId,
        EventId = e.EventId,
        EventName = e.Event?.EventName ?? string.Empty,
        EventDate = e.Event?.EventDate ?? default,
        Location = e.Event?.Location ?? string.Empty,
        BannerUrl = e.Event?.BannerUrl,
        EventCategoryId = e.EventCategoryId,
        CategoryName = e.EventCategory?.Category?.CategoryName ?? string.Empty,
        EntryFee = e.EventCategory?.EntryFee ?? 0,
        Status = e.Status,
        PaymentStatus = e.PaymentStatus,
        BibNumber = e.BibNumber,
        EnrolmentDate = e.EnrolmentDate
    };

    public static OrganiserEnrolmentDto ToOrganiserDto(Enrolment e, Result? result) => new()
    {
        EnrolmentId = e.EnrolmentId,
        EventId = e.EventId,
        ParticipantId = e.ParticipantId,
        ParticipantName = FullName(e.Participant),
        ParticipantEmail = e.Participant?.Email ?? string.Empty,
        EventCategoryId = e.EventCategoryId,
        CategoryName = e.EventCategory?.Category?.CategoryName ?? string.Empty,
        Status = e.Status,
        PaymentStatus = e.PaymentStatus,
        BibNumber = e.BibNumber,
        EnrolmentDate = e.EnrolmentDate,
        Result = result is null ? null : ResultMapper.ToDto(result, e)
    };
}

public static class ResultMapper
{
    public static ResultDto ToDto(Result r, Enrolment? enrolment = null)
    {
        var e = enrolment ?? r.Enrolment;
        return new ResultDto
        {
            ResultId = r.ResultId,
            EnrolmentId = r.EnrolmentId,
            ParticipantName = EnrolmentMapper.FullName(e?.Participant),
            BibNumber = e?.BibNumber,
            CategoryName = e?.EventCategory?.Category?.CategoryName ?? string.Empty,
            FinishTime = r.FinishTime,
            Position = r.Position,
            OverallRank = r.OverallRank,
            CategoryRank = r.CategoryRank,
            PacePerKm = r.PacePerKm,
            Verified = r.Verified
        };
    }
}
