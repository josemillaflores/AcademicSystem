using Microsoft.EntityFrameworkCore;
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
}