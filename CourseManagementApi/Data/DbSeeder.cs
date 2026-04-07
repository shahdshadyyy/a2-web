using CourseManagementApi.Entities;
using CourseManagementApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var instUser1 = Guid.Parse("22222222-2222-2222-2222-222222222221");
        var instUser2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var studUser1 = Guid.Parse("33333333-3333-3333-3333-333333333331");
        var studUser2 = Guid.Parse("33333333-3333-3333-3333-333333333332");

        var instructor1Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var instructor2Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var student1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
        var student2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");

        var course1 = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
        var course2 = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");
        var course3 = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");

        var users = new List<User>
        {
            new()
            {
                Id = adminId,
                FullName = "System Administrator",
                Email = "admin@university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new()
            {
                Id = instUser1,
                FullName = "Dr. Alice Johnson",
                Email = "alice.johnson@university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Instructor123!"),
                Role = UserRole.Instructor,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new()
            {
                Id = instUser2,
                FullName = "Prof. Bob Smith",
                Email = "bob.smith@university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Instructor123!"),
                Role = UserRole.Instructor,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new()
            {
                Id = studUser1,
                FullName = "Charlie Brown",
                Email = "charlie.brown@student.university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                Id = studUser2,
                FullName = "Dana Lee",
                Email = "dana.lee@student.university.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        await context.Users.AddRangeAsync(users);

        var instructors = new List<Instructor>
        {
            new()
            {
                Id = instructor1Id,
                UserId = instUser1,
                Department = "Computer Science",
                HireDate = DateTime.UtcNow.AddYears(-5).Date
            },
            new()
            {
                Id = instructor2Id,
                UserId = instUser2,
                Department = "Mathematics",
                HireDate = DateTime.UtcNow.AddYears(-3).Date
            }
        };

        await context.Instructors.AddRangeAsync(instructors);

        var profile = new InstructorProfile
        {
            Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
            InstructorId = instructor1Id,
            Bio = "Specializes in software engineering and distributed systems.",
            OfficeLocation = "Science Hall 204",
            PhoneNumber = "+1-555-0100"
        };

        await context.InstructorProfiles.AddAsync(profile);

        var students = new List<Student>
        {
            new()
            {
                Id = student1Id,
                UserId = studUser1,
                UniversityId = "U-10001",
                Level = "Junior"
            },
            new()
            {
                Id = student2Id,
                UserId = studUser2,
                UniversityId = "U-10002",
                Level = "Senior"
            }
        };

        await context.Students.AddRangeAsync(students);

        var courses = new List<Course>
        {
            new()
            {
                Id = course1,
                Title = "Advanced Software Engineering",
                Code = "CS-401",
                Description = "Design patterns, architecture, and team development practices.",
                CreditHours = 4,
                InstructorId = instructor1Id
            },
            new()
            {
                Id = course2,
                Title = "Database Systems",
                Code = "CS-350",
                Description = "Relational theory, SQL, and transaction processing.",
                CreditHours = 3,
                InstructorId = instructor1Id
            },
            new()
            {
                Id = course3,
                Title = "Linear Algebra",
                Code = "MATH-220",
                Description = "Vectors, matrices, eigenvalues, and applications.",
                CreditHours = 3,
                InstructorId = instructor2Id
            }
        };

        await context.Courses.AddRangeAsync(courses);

        var enrollments = new List<Enrollment>
        {
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1"),
                StudentId = student1Id,
                CourseId = course1,
                EnrolledAt = DateTime.UtcNow.AddDays(-30),
                Grade = 88.5m
            },
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee2"),
                StudentId = student1Id,
                CourseId = course2,
                EnrolledAt = DateTime.UtcNow.AddDays(-20),
                Grade = null
            },
            new()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee3"),
                StudentId = student2Id,
                CourseId = course3,
                EnrolledAt = DateTime.UtcNow.AddDays(-10),
                Grade = 92m
            }
        };

        await context.Enrollments.AddRangeAsync(enrollments);
        await context.SaveChangesAsync();
    }
}
