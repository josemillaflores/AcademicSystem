using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data.Configurations;

namespace PaymentService.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}

public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public NullableUtcDateTimeConverter() : base(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : v.Value.ToUniversalTime()) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    {
    }
}