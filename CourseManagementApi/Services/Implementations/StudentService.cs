using System.Linq.Expressions;
using CourseManagementApi.Data;
using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.DTOs.Student;
using CourseManagementApi.Entities;
using CourseManagementApi.Enums;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public StudentService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<StudentReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsAdmin)
        {
            return await _context.Students.AsNoTracking()
                .OrderBy(s => s.User.FullName)
                .Select(MapStudentRead())
                .ToListAsync(cancellationToken);
        }

        if (_currentUser.IsStudent && _currentUser.UserId is Guid uid)
        {
            return await _context.Students.AsNoTracking()
                .Where(s => s.UserId == uid)
                .Select(MapStudentRead())
                .ToListAsync(cancellationToken);
        }

        throw new ForbiddenException("You are not allowed to list students.");
    }

    public async Task<StudentReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _context.Students.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(MapStudentRead())
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return null;

        if (_currentUser.IsAdmin)
            return dto;

        if (_currentUser.IsStudent && _currentUser.UserId == dto.UserId)
            return dto;

        throw new ForbiddenException("You are not allowed to view this student.");
    }

    public async Task<StudentReadDto> CreateAsync(CreateStudentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only administrators can create students.");

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new BadRequestException("A user with this email already exists.");

        if (await _context.Students.AnyAsync(s => s.UniversityId == dto.UniversityId, cancellationToken))
            throw new BadRequestException("University ID must be unique.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
        };

        var student = new Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            UniversityId = dto.UniversityId,
            Level = dto.Level
        };

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.Students.AddAsync(student, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await GetByIdAsync(student.Id, cancellationToken);
        return created ?? throw new InvalidOperationException("Failed to load created student.");
    }

    public async Task<StudentReadDto?> UpdateAsync(Guid id, UpdateStudentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
        {
            var owns = await _context.Students.AsNoTracking()
                .AnyAsync(s => s.UserId == _currentUser.UserId && s.Id == id, cancellationToken);

            if (!owns)
                throw new ForbiddenException("You can only update your own student record.");
        }

        if (dto.UniversityId != null &&
            await _context.Students.AnyAsync(s => s.UniversityId == dto.UniversityId && s.Id != id, cancellationToken))
            throw new BadRequestException("University ID must be unique.");

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (student == null)
            return null;

        if (dto.FullName != null)
            student.User.FullName = dto.FullName;
        if (dto.UniversityId != null)
            student.UniversityId = dto.UniversityId;
        if (dto.Level != null)
            student.Level = dto.Level;

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Students.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(MapStudentRead())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAdmin)
            throw new ForbiddenException("Only administrators can delete students.");

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (student == null)
            throw new NotFoundException("Student not found.");

        _context.Users.Remove(student.User);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnrollmentReadDto>> GetEnrollmentsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var ownerRow = await _context.Students.AsNoTracking()
            .Where(s => s.Id == studentId)
            .Select(s => new { s.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerRow == null)
            throw new NotFoundException("Student not found.");

        var ownerUserId = ownerRow.UserId;

        if (!_currentUser.IsAdmin && !(_currentUser.IsStudent && _currentUser.UserId == ownerUserId))
            throw new ForbiddenException("You can only view your own enrollments.");

        return await _context.Enrollments.AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentReadDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student.User.FullName,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                CourseCode = e.Course.Code,
                EnrolledAt = e.EnrolledAt,
                Grade = e.Grade
            })
            .ToListAsync(cancellationToken);
    }

    private static Expression<Func<Student, StudentReadDto>> MapStudentRead() =>
        s => new StudentReadDto
        {
            Id = s.Id,
            UserId = s.UserId,
            FullName = s.User.FullName,
            Email = s.User.Email,
            UniversityId = s.UniversityId,
            Level = s.Level
        };
}
