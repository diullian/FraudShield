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
              .OwnsOne(t => t.Customer);

        modelBuilder.Entity<FinancialTransaction>()
              .OwnsOne(t => t.Merchant);


    }
}
