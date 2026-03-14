using FraudShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FraudShield.Infrastructure.DataAccess;

public class FraudShieldDbContext : DbContext
{
    public FraudShieldDbContext(DbContextOptions options) : base(options) { }

    DbSet<FinancialTransaction> FinancialTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<FinancialTransaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);
      

    }
}
