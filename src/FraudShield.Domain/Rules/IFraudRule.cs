using FraudShield.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Domain.Rules;

public interface IFraudRule
{

    public string Name { get; }
    RuleResult Evaluate(FinancialTransaction transaction);

}