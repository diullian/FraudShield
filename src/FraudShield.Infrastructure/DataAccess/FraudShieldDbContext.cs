using FraudShield.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FraudShield.Infrastructure.DataAccess;

public class FraudShieldDbContext : DbContext
{
    public FraudShieldDbContext(DbContextOptions options) : base(options) { }

    DbSet<FinancialTransaction> FinancialTransactions { get; set; }
}
