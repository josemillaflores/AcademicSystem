using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(p => p.PaymentNumber)
            .HasColumnName("PaymentNumber")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.HasIndex(p => p.PaymentNumber).IsUnique();
        
        builder.Property(p => p.StudentId)
            .HasColumnName("StudentId")
            .IsRequired();
        
        builder.Property(p => p.StudentName)
            .HasColumnName("StudentName")
            .HasMaxLength(200);
        
        builder.Property(p => p.StudentNumber)
            .HasColumnName("StudentNumber")
            .HasMaxLength(20);
        
        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(a => a.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();
            
            amount.Property(a => a.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");
        });
        
        builder.Property(p => p.PaymentDate)
            .HasColumnName("PaymentDate")
            .IsRequired();
        
        builder.Property(p => p.Method)
            .HasColumnName("Method")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(p => p.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(p => p.GatewayResponse)
            .HasColumnName("GatewayResponse")
            .HasMaxLength(500);
        
        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt");
        
        // Relaciones
        builder.HasMany(p => p.Transactions)
            .WithOne()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índices
        builder.HasIndex(p => p.StudentId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.Method);
    }
}