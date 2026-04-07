using CourseManagementApi.Data;
using CourseManagementApi.DTOs.Course;
using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.Entities;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CourseService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CourseReadDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Courses.AsNoTracking();

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var instructorId = await _context.Instructors.AsNoTracking()
                .Where(i => i.UserId == uid)
                .Select(i => i.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (instructorId == Guid.Empty)
                return Array.Empty<CourseReadDto>();

            query = query.Where(c => c.InstructorId == instructorId);
        }
        else if (!_currentUser.IsAdmin && !_currentUser.IsStudent)
        {
            throw new ForbiddenException("You are not allowed to list courses.");
        }

        return await query
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

    public async Task<CourseReadDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _context.Courses.AsNoTracking()
            .Where(c => c.Id == id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
            return null;

        if (_currentUser.IsAdmin || _currentUser.IsStudent)
            return dto;

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var owns = await _context.Instructors.AsNoTracking()
                .AnyAsync(i => i.UserId == uid && i.Id == dto.InstructorId, cancellationToken);

            if (owns)
                return dto;
        }

        throw new ForbiddenException("You are not allowed to view this course.");
    }

    public async Task<CourseReadDto> CreateAsync(CreateCourseDto dto, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var myInstructorId = await _context.Instructors.AsNoTracking()
                .Where(i => i.UserId == uid)
                .Select(i => i.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (myInstructorId == Guid.Empty || myInstructorId != dto.InstructorId)
                throw new ForbiddenException("Instructors can only create courses assigned to themselves.");
        }
        else if (!_currentUser.IsAdmin)
        {
            throw new ForbiddenException("You are not allowed to create courses.");
        }

        if (!await _context.Instructors.AnyAsync(i => i.Id == dto.InstructorId, cancellationToken))
            throw new BadRequestException("Instructor not found.");

        if (await _context.Courses.AnyAsync(c => c.Code == dto.Code, cancellationToken))
            throw new BadRequestException("Course code must be unique.");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Code = dto.Code,
            Description = dto.Description,
            CreditHours = dto.CreditHours,
            InstructorId = dto.InstructorId
        };

        await _context.Courses.AddAsync(course, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await GetByIdAsync(course.Id, cancellationToken);
        return created ?? throw new InvalidOperationException("Failed to load created course.");
    }

    public async Task<CourseReadDto?> UpdateAsync(Guid id, UpdateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (course == null)
            return null;

        await EnsureCanModifyCourseAsync(course.InstructorId, cancellationToken);

        if (dto.Code != null &&
            await _context.Courses.AnyAsync(c => c.Code == dto.Code && c.Id != id, cancellationToken))
            throw new BadRequestException("Course code must be unique.");

        if (dto.InstructorId.HasValue)
        {
            if (!await _context.Instructors.AnyAsync(i => i.Id == dto.InstructorId.Value, cancellationToken))
                throw new BadRequestException("Instructor not found.");

            if (_currentUser.IsInstructor)
                throw new ForbiddenException("Instructors cannot reassign a course to another instructor.");

            course.InstructorId = dto.InstructorId.Value;
        }

        if (dto.Title != null)
            course.Title = dto.Title;
        if (dto.Code != null)
            course.Code = dto.Code;
        if (dto.Description != null)
            course.Description = dto.Description;
        if (dto.CreditHours.HasValue)
            course.CreditHours = dto.CreditHours.Value;

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Courses.AsNoTracking()
            .Where(c => c.Id == id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (course == null)
            throw new NotFoundException("Course not found.");

        await EnsureCanModifyCourseAsync(course.InstructorId, cancellationToken);

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EnrollmentReadDto>> GetStudentsAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.InstructorId })
            .FirstOrDefaultAsync(cancellationToken);

        if (course == null)
            throw new NotFoundException("Course not found.");

        if (_currentUser.IsAdmin)
        {
            return await QueryEnrollmentsForCourse(courseId).ToListAsync(cancellationToken);
        }

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var owns = await _context.Instructors.AsNoTracking()
                .AnyAsync(i => i.UserId == uid && i.Id == course.InstructorId, cancellationToken);

            if (owns)
                return await QueryEnrollmentsForCourse(courseId).ToListAsync(cancellationToken);
        }

        throw new ForbiddenException("You are not allowed to view enrollments for this course.");
    }

    private IQueryable<EnrollmentReadDto> QueryEnrollmentsForCourse(Guid courseId) =>
        _context.Enrollments.AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Student.User.FullName)
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
            });

    private async Task EnsureCanModifyCourseAsync(Guid instructorId, CancellationToken cancellationToken)
    {
        if (_currentUser.IsAdmin)
            return;

        if (_currentUser.IsInstructor && _currentUser.UserId is Guid uid)
        {
            var owns = await _context.Instructors.AsNoTracking()
                .AnyAsync(i => i.UserId == uid && i.Id == instructorId, cancellationToken);

            if (owns)
                return;
        }

        throw new ForbiddenException("You are not allowed to modify this course.");
    }
}
