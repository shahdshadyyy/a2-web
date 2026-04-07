using System.Linq.Expressions;
using CourseManagementApi.Data;
using CourseManagementApi.DTOs.Course;
using CourseManagementApi.DTOs.Instructor;
using CourseManagementApi.Entities;
using CourseManagementApi.Enums;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Services.Implementations;

public class InstructorService : IInstructorService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public InstructorService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InstructorReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsAdmin)
        {
            return await _context.Instructors.AsNoTracking()
                .OrderBy(i => i.User.FullName)
                .Select(MapInstructorRead())
                .ToListAsync(cancellationToken);
        }

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            return await _context.Instructors.AsNoTracking()
                .Where(i => i.UserId == uid)
                .Select(MapInstructorRead())
                .ToListAsync(cancellationToken);
        }

        throw new ForbiddenException("You are not allowed to list instructors.");
    }

    public async Task<InstructorReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _context.Instructors.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(MapInstructorRead())
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return null;

        if (_currentUser.IsAdmin)
            return dto;

        if (_currentUser.IsInstructor && _currentUser.UserId == dto.UserId)
            return dto;

        throw new ForbiddenException("You are not allowed to view this instructor.");
    }

    public async Task<InstructorReadDto> CreateAsync(CreateInstructorDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only administrators can create instructors.");

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new BadRequestException("A user with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Instructor,
            CreatedAt = DateTime.UtcNow
        };

        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Department = dto.Department,
            HireDate = dto.HireDate.Date
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.Instructors.AddAsync(instructor, cancellationToken);

        if (!string.IsNullOrWhiteSpace(dto.Bio) || !string.IsNullOrWhiteSpace(dto.OfficeLocation) ||
            !string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            await _context.InstructorProfiles.AddAsync(new InstructorProfile
            {
                Id = Guid.NewGuid(),
                InstructorId = instructor.Id,
                Bio = dto.Bio,
                OfficeLocation = dto.OfficeLocation,
                PhoneNumber = dto.PhoneNumber
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var created = await GetByIdAsync(instructor.Id, cancellationToken);
        return created ?? throw new InvalidOperationException("Failed to load created instructor.");
    }

    public async Task<InstructorReadDto?> UpdateAsync(Guid id, UpdateInstructorDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
        {
            var ownsProfile = await _context.Instructors.AsNoTracking()
                .AnyAsync(i => i.UserId == _currentUser.UserId && i.Id == id, cancellationToken);

            if (!ownsProfile)
                throw new ForbiddenException("You can only update your own instructor profile.");
        }

        var instructor = await _context.Instructors
            .Include(i => i.User)
            .Include(i => i.InstructorProfile)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (instructor == null)
            return null;

        if (dto.FullName != null)
            instructor.User.FullName = dto.FullName;
        if (dto.Department != null)
            instructor.Department = dto.Department;
        if (dto.HireDate.HasValue)
            instructor.HireDate = dto.HireDate.Value.Date;

        if (dto.Bio != null || dto.OfficeLocation != null || dto.PhoneNumber != null)
        {
            instructor.InstructorProfile ??= new InstructorProfile
            {
                Id = Guid.NewGuid(),
                InstructorId = instructor.Id
            };

            if (dto.Bio != null)
                instructor.InstructorProfile.Bio = dto.Bio;
            if (dto.OfficeLocation != null)
                instructor.InstructorProfile.OfficeLocation = dto.OfficeLocation;
            if (dto.PhoneNumber != null)
                instructor.InstructorProfile.PhoneNumber = dto.PhoneNumber;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Instructors.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(MapInstructorRead())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only administrators can delete instructors.");

        var hasCourses = await _context.Courses.AsNoTracking()
            .AnyAsync(c => c.InstructorId == id, cancellationToken);

        if (hasCourses)
            throw new BadRequestException("Reassign or delete courses before removing this instructor.");

        var instructor = await _context.Instructors
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (instructor == null)
            throw new NotFoundException("Instructor not found.");

        _context.Users.Remove(instructor.User);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseReadDto>> GetCoursesAsync(Guid instructorId, CancellationToken cancellationToken = default)
    {
        var instructorUserId = await _context.Instructors.AsNoTracking()
            .Where(i => i.Id == instructorId)
            .Select(i => new { i.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (instructorUserId == null)
            throw new NotFoundException("Instructor not found.");

        if (!_currentUser.IsAdmin && !(_currentUser.IsInstructor && _currentUser.UserId == instructorUserId.UserId))
            throw new ForbiddenException("You can only view courses for your own instructor account.");

        return await _context.Courses.AsNoTracking()
            .Where(c => c.InstructorId == instructorId)
            .OrderBy(c => c.Code)
            .Select(c => new CourseReadDto
            {
                Id = c.Id,
                Title = c.Title,
                Code = c.Code,
                Description = c.Description,
                CreditHours = c.CreditHours,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor.User.FullName
            })
            .ToListAsync(cancellationToken);
    }

    private static Expression<Func<Instructor, InstructorReadDto>> MapInstructorRead()
    {
        return i => new InstructorReadDto
        {
            Id = i.Id,
            UserId = i.UserId,
            FullName = i.User.FullName,
            Email = i.User.Email,
            Department = i.Department,
            HireDate = i.HireDate,
            Profile = i.InstructorProfile == null
                ? null
                : new InstructorProfileDto
                {
                    Id = i.InstructorProfile.Id,
                    Bio = i.InstructorProfile.Bio,
                    OfficeLocation = i.InstructorProfile.OfficeLocation,
                    PhoneNumber = i.InstructorProfile.PhoneNumber
                }
        };
    }
}
