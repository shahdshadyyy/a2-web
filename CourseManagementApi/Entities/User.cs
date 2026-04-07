using CourseManagementApi.Enums;

namespace CourseManagementApi.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public Instructor? Instructor { get; set; }
    public Student? Student { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
