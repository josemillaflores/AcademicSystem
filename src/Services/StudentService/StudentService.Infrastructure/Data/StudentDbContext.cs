using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AcademicSystem.Common.Entities;
using StudentService.Domain.Entities;
using StudentService.Infrastructure.Data.Configurations;

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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
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
            var entity = (BaseEntity)entityEntry.Entity;
            
            if (entityEntry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}