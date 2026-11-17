using System.Globalization;
using ST10335260_PROG6212_POE_Part3.Models;

namespace ST10335260_PROG6212_POE_Part3.Helpers;

public static class Format
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Ordinal(int number)
    {
        var lastTwo = number % 100;
        if (lastTwo is >= 11 and <= 13) return $"{number}th";

        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }

    public static string FinishTime(TimeSpan time) => time.ToString(@"hh\:mm\:ss", Culture);

    public static string Pace(decimal? minutesPerKm)
    {
        if (minutesPerKm is null || minutesPerKm <= 0) return "-";
        var totalSeconds = (int)Math.Round(minutesPerKm.Value * 60);
        return $"{totalSeconds / 60}:{totalSeconds % 60:00} /km";
    }

    public static string Money(decimal amount) =>
        amount == decimal.Truncate(amount)
            ? "R" + amount.ToString("N0", Culture)
            : "R" + amount.ToString("N2", Culture);

    public static string Distance(decimal? km) =>
        km is null ? "-" : km.Value.ToString("0.##", Culture) + " km";

    public static string LongDate(DateTime date) => date.ToString("dddd, d MMMM yyyy", Culture);

    public static string ShortDate(DateTime date) => date.ToString("d MMM yyyy", Culture);

    public static string Month(DateTime date) => date.ToString("MMM", Culture);

    public static string Time(DateTime date) => date.ToString("HH:mm", Culture);

    public static string StatusClass(EnrolmentStatus status) => status switch
    {
        EnrolmentStatus.Confirmed => "status-confirmed",
        EnrolmentStatus.Pending => "status-pending",
        _ => "status-cancelled"
    };

    public static string StatusIcon(EnrolmentStatus status) => status switch
    {
        EnrolmentStatus.Confirmed => "bi-check-circle-fill",
        EnrolmentStatus.Pending => "bi-hourglass-split",
        _ => "bi-x-circle-fill"
    };

    public static string TypeIcon(EventType type) => type switch
    {
        EventType.Walk => "bi-person-walking",
        EventType.Cycle => "bi-bicycle",
        _ => "bi-lightning-charge-fill"
    };

    public static string TypeClass(EventType type) => type.ToString().ToLowerInvariant();

    public static string DateTimeLocal(DateTime date) => date.ToString("yyyy-MM-ddTHH:mm", Culture);
}
