using CourseManagementApi.Data;
using CourseManagementApi.DTOs.User;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UserReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only administrators can list all users.");

        return await _context.Users.AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new UserReadDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null)
            throw new ForbiddenException("Authentication required.");

        if (!_currentUser.IsAdmin && _currentUser.UserId != id)
            throw new ForbiddenException("You can only view your own user record.");

        return await _context.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserReadDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
