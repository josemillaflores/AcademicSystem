using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherService.Domain.Entities;

namespace TeacherService.Infrastructure.Data.Configurations;

public class CourseAssignmentConfiguration : IEntityTypeConfiguration<CourseAssignment>
{
    public void Configure(EntityTypeBuilder<CourseAssignment> builder)
    {
        builder.ToTable("CourseAssignments");
        
        builder.HasKey(ca => ca.Id);
        
        builder.Property(ca => ca.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(ca => ca.CourseId)
            .HasColumnName("CourseId")
            .IsRequired();
        
        builder.Property(ca => ca.CourseName)
            .HasColumnName("CourseName")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(ca => ca.Credits)
            .HasColumnName("Credits")
            .IsRequired();
        
        builder.Property(ca => ca.HoursPerWeek)
            .HasColumnName("HoursPerWeek")
            .IsRequired();
        
        builder.Property(ca => ca.StudentsCount)
            .HasColumnName("StudentsCount")
            .HasDefaultValue(0);
        
        builder.Property(ca => ca.AssignmentDate)
            .HasColumnName("AssignmentDate")
            .IsRequired();
        
        builder.Property(ca => ca.Period)
            .HasColumnName("Period")
            .HasMaxLength(20);
        
        builder.Property(ca => ca.IsActive)
            .HasColumnName("IsActive")
            .HasDefaultValue(true);
        
        builder.Property(ca => ca.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(ca => ca.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        builder.Property(ca => ca.TeacherId)
            .HasColumnName("TeacherId")
            .IsRequired();
        
        // Índices
        builder.HasIndex(ca => ca.CourseId);
        builder.HasIndex(ca => ca.Period);
        builder.HasIndex(ca => ca.IsActive);
        builder.HasIndex(ca => new { ca.TeacherId, ca.CourseId, ca.Period }).IsUnique();
    }
}