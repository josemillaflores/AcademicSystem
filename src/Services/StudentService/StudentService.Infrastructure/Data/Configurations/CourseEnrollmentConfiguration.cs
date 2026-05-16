using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentService.Domain.Entities;

namespace StudentService.Infrastructure.Data.Configurations;

public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("CourseEnrollments");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(e => e.CourseId)
            .HasColumnName("CourseId")
            .IsRequired();
        
        builder.Property(e => e.CourseName)
            .HasColumnName("CourseName")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(e => e.Credits)
            .HasColumnName("Credits")
            .IsRequired();
        
        builder.Property(e => e.EnrollmentDate)
            .HasColumnName("EnrollmentDate")
            .IsRequired();
        
        builder.Property(e => e.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.Grade)
            .HasColumnName("Grade")
            .HasPrecision(3, 2);
        
        builder.Property(e => e.StudentId)
            .HasColumnName("StudentId")
            .IsRequired();
        
        // Índices
        builder.HasIndex(e => e.CourseId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
    }
}