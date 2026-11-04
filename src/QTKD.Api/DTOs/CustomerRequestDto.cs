using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public class CustomerRequestDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;
}
