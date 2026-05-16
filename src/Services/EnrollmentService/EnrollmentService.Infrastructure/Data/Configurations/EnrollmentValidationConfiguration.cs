using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Infrastructure.Data.Configurations;

public class EnrollmentValidationConfiguration : IEntityTypeConfiguration<EnrollmentValidation>
{
    public void Configure(EntityTypeBuilder<EnrollmentValidation> builder)
    {
        builder.ToTable("EnrollmentValidations");
        
        builder.HasKey(ev => ev.Id);
        
        builder.Property(ev => ev.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(ev => ev.Type)
            .HasColumnName("Type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        
        builder.Property(ev => ev.IsValid)
            .HasColumnName("IsValid")
            .IsRequired();
        
        builder.Property(ev => ev.Message)
            .HasColumnName("Message")
            .HasMaxLength(500)
            .IsRequired();
        
        builder.Property(ev => ev.ValidatedAt)
            .HasColumnName("ValidatedAt")
            .IsRequired();
        
        builder.Property(ev => ev.EnrollmentId)
            .HasColumnName("EnrollmentId")
            .IsRequired();
        
        // Índices
        builder.HasIndex(ev => ev.EnrollmentId);
        builder.HasIndex(ev => ev.Type);
    }
}