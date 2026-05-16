using Microsoft.EntityFrameworkCore;
using TeacherService.Domain.Entities;
using TeacherService.Infrastructure.Data.Configurations;

namespace TeacherService.Infrastructure.Data;

public class TeacherDbContext : DbContext
{
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<CourseAssignment> CourseAssignments { get; set; }

    public TeacherDbContext(DbContextOptions<TeacherDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TeacherConfiguration());
        modelBuilder.ApplyConfiguration(new SpecialtyConfiguration());
        modelBuilder.ApplyConfiguration(new CourseAssignmentConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}