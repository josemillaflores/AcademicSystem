using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CourseService.Domain.Entities;

namespace CourseService.Infrastructure.Data.Configurations;

public class PrerequisiteConfiguration : IEntityTypeConfiguration<Prerequisite>
{
    public void Configure(EntityTypeBuilder<Prerequisite> builder)
    {
        builder.ToTable("Prerequisites");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(p => p.CourseId)
            .HasColumnName("CourseId")
            .IsRequired();
        
        builder.Property(p => p.RequiredCourseId)
            .HasColumnName("RequiredCourseId")
            .IsRequired();
        
        builder.Property(p => p.RequiredCourseName)
            .HasColumnName("RequiredCourseName")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(p => p.RequiredCourseCode)
            .HasColumnName("RequiredCourseCode")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(p => p.IsMandatory)
            .HasColumnName("IsMandatory")
            .HasDefaultValue(true);
        
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        // Índices
        builder.HasIndex(p => p.CourseId);
        builder.HasIndex(p => p.RequiredCourseId);
        builder.HasIndex(p => new { p.CourseId, p.RequiredCourseId }).IsUnique();
    }
}