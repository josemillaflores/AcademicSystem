using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourseService.Domain.Entities;

namespace CourseService.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(c => c.Code)
            .HasColumnName("Code")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.HasIndex(c => c.Code).IsUnique();
        
        builder.Property(c => c.Name)
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(c => c.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000);
        
        builder.Property(c => c.Credits)
            .HasColumnName("Credits")
            .IsRequired();
        
        builder.Property(c => c.TotalHours)
            .HasColumnName("TotalHours")
            .IsRequired();
        
        builder.Property(c => c.MaxCapacity)
            .HasColumnName("MaxCapacity")
            .IsRequired();
        
        builder.Property(c => c.CurrentEnrollment)
            .HasColumnName("CurrentEnrollment")
            .HasDefaultValue(0);
        
        builder.Property(c => c.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        // Value Object: Schedule
        builder.OwnsOne(c => c.Schedule, schedule =>
        {
            schedule.Property(s => s.Day)
                .HasColumnName("ScheduleDay")
                .HasConversion<string>()
                .HasMaxLength(20);
            
            schedule.Property(s => s.StartTime)
                .HasColumnName("StartTime");
            
            schedule.Property(s => s.EndTime)
                .HasColumnName("EndTime");
            
            schedule.Property(s => s.Classroom)
                .HasColumnName("Classroom")
                .HasMaxLength(50);
        });
        
        builder.Property(c => c.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(c => c.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        // Relaciones
        builder.HasMany(c => c.Prerequisites)
            .WithOne()
            .HasForeignKey("CourseId")
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Credits);
    }
}