using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CourseService.Domain.Entities;
using CourseService.Infrastructure.Data.Configurations;

namespace CourseService.Infrastructure.Data;

public class CourseDbContext : DbContext
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<Prerequisite> Prerequisites { get; set; }

    public CourseDbContext(DbContextOptions<CourseDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new PrerequisiteConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
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