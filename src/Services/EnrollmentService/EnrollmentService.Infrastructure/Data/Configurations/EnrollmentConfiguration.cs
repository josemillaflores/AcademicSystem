using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(e => e.StudentId)
            .HasColumnName("StudentId")
            .IsRequired();
        
        builder.Property(e => e.CourseId)
            .HasColumnName("CourseId")
            .IsRequired();
        
        builder.Property(e => e.StudentName)
            .HasColumnName("StudentName")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(e => e.StudentNumber)
            .HasColumnName("StudentNumber")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.CourseName)
            .HasColumnName("CourseName")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(e => e.CourseCode)
            .HasColumnName("CourseCode")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.EnrollmentDate)
            .HasColumnName("EnrollmentDate")
            .IsRequired();
        
        // Value Object: EnrollmentPeriod
        builder.OwnsOne(e => e.Period, period =>
        {
            period.Property(p => p.Name)
                .HasColumnName("PeriodName")
                .HasMaxLength(20)
                .IsRequired();
            
            period.Property(p => p.StartDate)
                .HasColumnName("PeriodStartDate");
            
            period.Property(p => p.EndDate)
                .HasColumnName("PeriodEndDate");
        });
        
        builder.Property(e => e.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(e => e.RejectionReason)
            .HasColumnName("RejectionReason")
            .HasMaxLength(500);
        
        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        // Relaciones
        builder.HasMany(e => e.Validations)
            .WithOne()
            .HasForeignKey("EnrollmentId")
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(e => e.StudentId);
        builder.HasIndex(e => e.CourseId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.EnrollmentDate);
        builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
    }
}