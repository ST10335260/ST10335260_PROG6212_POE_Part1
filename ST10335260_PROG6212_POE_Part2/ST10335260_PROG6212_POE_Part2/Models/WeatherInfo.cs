namespace ST10335260_PROG6212_POE_Part2.Models;

public class WeatherInfo
{
    public int WeatherId { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public DateTime ForecastDate { get; set; }
    public decimal? Temperature { get; set; }
    public string? Conditions { get; set; }
    public decimal? WindSpeed { get; set; }
}