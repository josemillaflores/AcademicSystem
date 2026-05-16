using Microsoft.EntityFrameworkCore;
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
}