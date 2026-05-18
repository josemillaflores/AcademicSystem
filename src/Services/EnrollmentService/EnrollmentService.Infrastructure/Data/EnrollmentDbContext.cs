using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using EnrollmentService.Domain.Entities;
using EnrollmentService.Infrastructure.Data.Configurations;

namespace EnrollmentService.Infrastructure.Data;

public class EnrollmentDbContext : DbContext
{
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<EnrollmentValidation> EnrollmentValidations { get; set; }

    public EnrollmentDbContext(DbContextOptions<EnrollmentDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new EnrollmentValidationConfiguration());
        
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