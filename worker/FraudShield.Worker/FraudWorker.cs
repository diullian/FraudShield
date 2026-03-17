using FraudShield.Worker.Contracts;
using FraudShield.Worker.Rules;
using FraudShield.Worker.Validation;
using MassTransit;

namespace FraudShield.Worker;

public class FraudWorker : IConsumer<TransactionCreatedEvent>
{
    private readonly ILogger<FraudWorker> _logger;
    private readonly IEventValidator _eventValidator;
    private readonly IRulesEngine _rulesEngine;
    private readonly IPublishEndpoint _publishEndpoint;

    public FraudWorker(ILogger<FraudWorker> logger, IEventValidator validator, IRulesEngine rulesEngine, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _eventValidator = validator;
        _rulesEngine = rulesEngine;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var transaction = context.Message;

        _logger.LogInformation("Received transaction: {TransactionId}", transaction.TransactionId);

        //valida se o contrato recebido é válido
        var validation = _eventValidator.Validate(transaction);

        if (!validation.IsValid)
        {

            _logger.LogWarning(
                "Invalid contract: {Errors}",
                string.Join(", ", validation.ErrorMessage));
            return;
        }
        

        //sendo válido o contrato, processa as regras de negócio
        var resultTransaction = await _rulesEngine.ValidateTransaction(transaction);

      
        if (resultTransaction.Decision == Enums.FraudDecision.Rejected)
        {
            _logger.LogWarning("Transaction {TransactionId} rejected due to high risk. Risk Level: {RiskLevel}",
                transaction.TransactionId, resultTransaction.RiskLevel);
        }
        else if(resultTransaction.Decision == Enums.FraudDecision.Review)
        {
            _logger.LogInformation("Transaction {TransactionId} flagged for manual review. Risk Level: {RiskLevel}",
                transaction.TransactionId, resultTransaction.RiskLevel);
        }
        else
        {
            _logger.LogInformation("Transaction {TransactionId} approved. Risk Level: {RiskLevel}",
                transaction.TransactionId, resultTransaction.RiskLevel);
        }

        var fraudFinalResult = new FraudEvaluatedResultEvent
        {
            TransactionId = resultTransaction.TransactionId,
            Status = resultTransaction.Decision.ToString(),
            RiskLevel = resultTransaction.RiskLevel.ToString(),
            CreatedAt = transaction.CreatedAt
        }; 

        //após processar as regras, publica o resultado para que outros serviços possam consumir
        await _publishEndpoint.Publish(fraudFinalResult);

    }

}
