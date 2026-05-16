using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeacherService.Domain.Entities;

namespace TeacherService.Infrastructure.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        // Value Object: TeacherName
        builder.OwnsOne(t => t.Name, name =>
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
        builder.OwnsOne(t => t.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(100)
                .IsRequired();
                
            email.HasIndex(e => e.Value).IsUnique();
        });
        
        // Value Object: TeacherId
        builder.OwnsOne(t => t.TeacherNumber, tn =>
        {
            tn.Property(n => n.Value)
                .HasColumnName("TeacherNumber")
                .HasMaxLength(20)
                .IsRequired();
                
            tn.HasIndex(n => n.Value).IsUnique();
        });
        
        // Value Object: AcademicLoad
        builder.OwnsOne(t => t.AcademicLoad, al =>
        {
            al.Property(a => a.MaxHoursPerWeek)
                .HasColumnName("MaxHoursPerWeek")
                .IsRequired()
                .HasDefaultValue(40);
                
            al.Property(a => a.CurrentHours)
                .HasColumnName("CurrentHours")
                .IsRequired()
                .HasDefaultValue(0);
        });
        
        // Propiedades simples
        builder.Property(t => t.HireDate)
            .HasColumnName("HireDate")
            .IsRequired();
        
        builder.Property(t => t.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(t => t.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(t => t.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        // Relaciones
        builder.HasMany(t => t.Specialties)
            .WithOne()
            .HasForeignKey("TeacherId")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(t => t.CourseAssignments)
            .WithOne()
            .HasForeignKey("TeacherId")
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.HireDate);
    }
}