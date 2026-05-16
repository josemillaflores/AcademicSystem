using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(t => t.TransactionId)
            .HasColumnName("TransactionId")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasIndex(t => t.TransactionId).IsUnique();
        
        builder.Property(t => t.Amount)
            .HasColumnName("Amount")
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(t => t.TransactionDate)
            .HasColumnName("TransactionDate")
            .IsRequired();
        
        builder.Property(t => t.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(t => t.GatewayResponse)
            .HasColumnName("GatewayResponse")
            .HasMaxLength(500);
        
        builder.Property(t => t.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
        
        builder.Property(t => t.PaymentId)
            .HasColumnName("PaymentId")
            .IsRequired();
        
        // Índices
        builder.HasIndex(t => t.PaymentId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.TransactionDate);
    }
}