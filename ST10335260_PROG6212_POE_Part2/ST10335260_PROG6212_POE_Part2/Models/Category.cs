namespace ST10335260_PROG6212_POE_Part2.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? DefaultDistance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}