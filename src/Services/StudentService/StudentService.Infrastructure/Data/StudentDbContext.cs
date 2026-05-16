using Microsoft.EntityFrameworkCore;
using StudentService.Domain.Entities;
using StudentService.Domain.ValueObjects;
using StudentService.Infrastructure.Data.Configurations;
using System.Reflection;

namespace StudentService.Infrastructure.Data;

public class StudentDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

    public StudentDbContext(DbContextOptions<StudentDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new CourseEnrollmentConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Actualizar timestamps automáticamente
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }
            ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}