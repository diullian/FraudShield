using FraudShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FraudShield.Infrastructure.DataAccess;

public class FraudShieldDbContext : DbContext
{
    public FraudShieldDbContext(DbContextOptions options) : base(options) { }

    public DbSet<FinancialTransaction> FinancialTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FinancialTransaction>()
              .Property(t => t.Amount)
              .HasPrecision(18, 2);

        modelBuilder.Entity<FinancialTransaction>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialTransaction>()
         .Property(t => t.RiskLevel)
         .HasConversion<string>();

        modelBuilder.Entity<FinancialTransaction>()
            .Property(t => t.PaymentType)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialTransaction>()
            .OwnsOne(t => t.Customer, c =>
            {
                c.Property(x => x.DeviceType)
                 .HasConversion<string>();
            });

        modelBuilder.Entity<FinancialTransaction>()
            .Property(t => t.Currency)
            .HasConversion<string>();

        modelBuilder.Entity<FinancialTransaction>()
            .OwnsOne(t => t.Merchant);

    }
}
