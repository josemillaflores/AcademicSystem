using Microsoft.EntityFrameworkCore;
using StudentService.Domain.Entities;

namespace StudentService.Infrastructure.Data;

public class StudentDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }

    public StudentDbContext(DbContextOptions<StudentDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.OwnsOne(s => s.Name, name =>
            {
                name.Property(n => n.FirstName).HasColumnName("FirstName").IsRequired();
                name.Property(n => n.LastName).HasColumnName("LastName").IsRequired();
            });
            
            entity.OwnsOne(s => s.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").IsRequired();
            });
            
            entity.OwnsOne(s => s.StudentNumber, sn =>
            {
                sn.Property(n => n.Value).HasColumnName("StudentNumber").IsRequired();
            });
            
            entity.OwnsOne(s => s.AcademicRecord, ar =>
            {
                ar.OwnsMany(a => a.Enrollments, enrollments =>
                {
                    enrollments.WithOwner().HasForeignKey("StudentId");
                    enrollments.Property<int>("Id");
                    enrollments.HasKey("Id");
                });
            });
        });
    }
}