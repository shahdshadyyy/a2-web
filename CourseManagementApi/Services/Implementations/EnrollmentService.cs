using System.Linq.Expressions;
using CourseManagementApi.Data;
using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.Entities;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EnrollmentService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<EnrollmentReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsAdmin)
        {
            return await QueryProjection()
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync(cancellationToken);
        }

        if (_currentUser.IsStudent && _currentUser.UserId is Guid uid)
        {
            var studentId = await _context.Students.AsNoTracking()
                .Where(s => s.UserId == uid)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (studentId == Guid.Empty)
                return Array.Empty<EnrollmentReadDto>();

            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(MapEnrollment())
                .ToListAsync(cancellationToken);
        }

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid iuid)
        {
            var instructorId = await _context.Instructors.AsNoTracking()
                .Where(i => i.UserId == iuid)
                .Select(i => i.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (instructorId == Guid.Empty)
                return Array.Empty<EnrollmentReadDto>();

            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.Course.InstructorId == instructorId)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(MapEnrollment())
                .ToListAsync(cancellationToken);
        }

        throw new ForbiddenException("You are not allowed to list enrollments.");
    }

    public async Task<EnrollmentReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _context.Enrollments.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(MapEnrollment())
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return null;

        await EnsureCanViewEnrollmentAsync(dto, cancellationToken);
        return dto;
    }

    public async Task<EnrollmentReadDto> CreateAsync(CreateEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _context.Students.AnyAsync(s => s.Id == dto.StudentId, cancellationToken))
            throw new BadRequestException("Student not found.");

        if (!await _context.Courses.AnyAsync(c => c.Id == dto.CourseId, cancellationToken))
            throw new BadRequestException("Course not found.");

        if (_currentUser.IsStudent)
        {
            var myStudentId = await _context.Students.AsNoTracking()
                .Where(s => s.UserId == _currentUser.UserId)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (myStudentId != dto.StudentId)
                throw new ForbiddenException("Students may only enroll themselves.");
        }
        else if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("You are not allowed to create enrollments.");
        }

        if (await _context.Enrollments.AnyAsync(
                e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId, cancellationToken))
            throw new BadRequestException("Student is already enrolled in this course.");

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(),
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            EnrolledAt = DateTime.UtcNow
        };

        await _context.Enrollments.AddAsync(enrollment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Enrollments.AsNoTracking()
            .Where(e => e.Id == enrollment.Id)
            .Select(MapEnrollment())
            .FirstAsync(cancellationToken);

        return created;
    }

    public async Task<EnrollmentReadDto?> UpdateAsync(Guid id, UpdateEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (enrollment == null)
            return null;

        if (dto.Grade == null)
            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.Id == id)
                .Select(MapEnrollment())
                .FirstOrDefaultAsync(cancellationToken);

        if (_currentUser.IsAdmin)
        {
            enrollment.Grade = dto.Grade;
            await _context.SaveChangesAsync(cancellationToken);
            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.Id == id)
                .Select(MapEnrollment())
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var ownsCourse = await _context.Instructors.AsNoTracking()
                .AnyAsync(i => i.UserId == uid && i.Id == enrollment.Course.InstructorId, cancellationToken);

            if (!ownsCourse)
                throw new ForbiddenException("You can only grade enrollments for your own courses.");

            enrollment.Grade = dto.Grade;
            await _context.SaveChangesAsync(cancellationToken);

            return await _context.Enrollments.AsNoTracking()
                .Where(e => e.Id == id)
                .Select(MapEnrollment())
                .FirstOrDefaultAsync(cancellationToken);
        }

        throw new ForbiddenException("You are not allowed to update this enrollment.");
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (enrollment == null)
            throw new NotFoundException("Enrollment not found.");

        if (_currentUser.IsAdmin)
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (_currentUser.IsStudent && enrollment.Student.UserId == _currentUser.UserId)
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var owns = await _context.Instructors.AsNoTracking()
                .AnyAsync(i => i.UserId == uid && i.Id == enrollment.Course.InstructorId, cancellationToken);

            if (owns)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }
        }

        throw new ForbiddenException("You are not allowed to delete this enrollment.");
    }

    private IQueryable<EnrollmentReadDto> QueryProjection() =>
        _context.Enrollments.AsNoTracking().Select(MapEnrollment());

    private static Expression<Func<Enrollment, EnrollmentReadDto>> MapEnrollment() =>
        e => new EnrollmentReadDto
        {
            Id = e.Id,
            StudentId = e.StudentId,
            StudentName = e.Student.User.FullName,
            CourseId = e.CourseId,
            CourseTitle = e.Course.Title,
            CourseCode = e.Course.Code,
            EnrolledAt = e.EnrolledAt,
            Grade = e.Grade
        };

    private async Task EnsureCanViewEnrollmentAsync(EnrollmentReadDto dto, CancellationToken cancellationToken)
    {
        if (_currentUser.IsAdmin)
            return;

        if (_currentUser.IsStudent && _currentUser.UserId is Guid suid)
        {
            var match = await _context.Students.AsNoTracking()
                .AnyAsync(s => s.UserId == suid && s.Id == dto.StudentId, cancellationToken);

            if (match)
                return;
        }

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid iuid)
        {
            var owns = await _context.Enrollments.AsNoTracking()
                .AnyAsync(e => e.Id == dto.Id && e.Course.Instructor.UserId == iuid, cancellationToken);

            if (owns)
                return;
        }

        throw new ForbiddenException("You are not allowed to view this enrollment.");
    }
}
