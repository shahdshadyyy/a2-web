using CourseManagementApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Department).HasMaxLength(150).IsRequired();
            entity.HasOne(e => e.User)
                .WithOne(u => u.Instructor)
                .HasForeignKey<Instructor>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Bio).HasMaxLength(2000);
            entity.Property(e => e.OfficeLocation).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(30);
            entity.HasOne(e => e.Instructor)
                .WithOne(i => i.InstructorProfile)
                .HasForeignKey<InstructorProfile>(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.InstructorId).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniversityId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Level).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(e => e.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Grade).HasPrecision(5, 2);
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.Token);
            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
