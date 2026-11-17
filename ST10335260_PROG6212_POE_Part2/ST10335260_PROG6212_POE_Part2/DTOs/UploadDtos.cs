using System.ComponentModel.DataAnnotations;

namespace ST10335260_PROG6212_POE_Part2.DTOs;

public class ImageUploadDto
{
    [Required]
    public IFormFile File { get; set; } = default!;
}

public class UploadResultDto
{
    public string Url { get; set; } = string.Empty;
}
