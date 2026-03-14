using FraudShield.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Entities;

public class FinancialTransaction
{

    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; }
    public decimal Amount { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Currency Currency { get; set; }
    public required PaymentType PaymentType { get; init; }
    public required Customer Customer { get; set; }
    public required Merchant Merchant { get; set; } 
    
}
