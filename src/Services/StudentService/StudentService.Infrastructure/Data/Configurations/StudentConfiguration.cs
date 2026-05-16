using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentService.Domain.Entities;
using StudentService.Domain.ValueObjects;

namespace StudentService.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        // Value Object: StudentName
        builder.OwnsOne(s => s.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(50)
                .IsRequired();
                
            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(50)
                .IsRequired();
                
            name.Property(n => n.MiddleName)
                .HasColumnName("MiddleName")
                .HasMaxLength(50);
        });
        
        // Value Object: Email
        builder.OwnsOne(s => s.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(100)
                .IsRequired();
                
            email.HasIndex(e => e.Value).IsUnique();
        });
        
        // Value Object: StudentNumber
        builder.OwnsOne(s => s.StudentNumber, sn =>
        {
            sn.Property(n => n.Value)
                .HasColumnName("StudentNumber")
                .HasMaxLength(20)
                .IsRequired();
                
            sn.HasIndex(n => n.Value).IsUnique();
        });
        
        // Value Object: ContactInfo
        builder.OwnsOne(s => s.ContactInfo, contact =>
        {
            contact.Property(c => c.Phone)
                .HasColumnName("Phone")
                .HasMaxLength(20);
                
            contact.Property(c => c.Address)
                .HasColumnName("Address")
                .HasMaxLength(200);
                
            contact.Property(c => c.City)
                .HasColumnName("City")
                .HasMaxLength(50);
                
            contact.Property(c => c.Country)
                .HasColumnName("Country")
                .HasMaxLength(50);
        });
        
        // Value Object: AcademicRecord
        builder.OwnsOne(s => s.AcademicRecord, ar =>
        {
            ar.Property(a => a.GPA)
                .HasColumnName("GPA")
                .HasPrecision(3, 2)
                .HasDefaultValue(0);
                
            ar.Property(a => a.TotalCredits)
                .HasColumnName("TotalCredits")
                .HasDefaultValue(0);
                
            ar.Property(a => a.RequiredCreditsForGraduation)
                .HasColumnName("RequiredCreditsForGraduation")
                .HasDefaultValue(180);
        });
        
        // Propiedades simples
        builder.Property(s => s.EnrollmentDate)
            .HasColumnName("EnrollmentDate")
            .IsRequired();
        
        builder.Property(s => s.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(s => s.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(s => s.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        // Relación con CourseEnrollment
        builder.HasMany(s => s.Enrollments)
            .WithOne()
            .HasForeignKey("StudentId")
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.EnrollmentDate);
    }
}