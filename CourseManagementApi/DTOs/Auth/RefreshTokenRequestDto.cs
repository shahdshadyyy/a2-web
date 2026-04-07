using System.ComponentModel.DataAnnotations;

namespace CourseManagementApi.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required]
    [MinLength(10)]
    [MaxLength(500)]
    public string RefreshToken { get; set; } = string.Empty;
}
