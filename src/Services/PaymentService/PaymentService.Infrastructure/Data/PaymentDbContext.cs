using Microsoft.EntityFrameworkCore;
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
}