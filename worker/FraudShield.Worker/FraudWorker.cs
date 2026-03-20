using FraudShield.Worker.Audit;
using FraudShield.Worker.Audit.Repository;
using FraudShield.Worker.Context;
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
    private readonly IAuditRepository _auditRepository;
    private readonly CorrelationContext _correlationContext;

    public FraudWorker(ILogger<FraudWorker> logger, IEventValidator validator, IRulesEngine rulesEngine, IPublishEndpoint publishEndpoint, IAuditRepository auditRepository, CorrelationContext correlationContext)
    {
        _logger = logger;
        _eventValidator = validator;
        _rulesEngine = rulesEngine;
        _publishEndpoint = publishEndpoint;
        _auditRepository = auditRepository;
        _correlationContext = correlationContext;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        if(context.CorrelationId == null)
        {
            _logger.LogWarning("Received message without CorrelationId. Message will be ignored.");
            throw new InvalidOperationException("CorrelationId is required for processing. Message will be ignored.");
        }

        //armazena o CorrelationId no contexto para que possa ser acessado em outros pontos do processamento
        _correlationContext.Initialize(context.CorrelationId?.ToString());

        var transaction = context.Message;

        _logger.LogInformation("Received transaction: {TransactionId} || CorrelationId {CorrelationId}", transaction.TransactionId, _correlationContext.CorrelationId);

        //valida se o contrato recebido é válido
        var validation = _eventValidator.Validate(transaction);

        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Invalid contract: {Errors}",
                string.Join(", ", validation.ErrorMessage));

           throw new InvalidOperationException($"Invalid contract: {string.Join(", ", validation.ErrorMessage)}");
        }
        
        //sendo válido o contrato, processa as regras de negócio
        var resultTransaction = await _rulesEngine.ValidateTransaction(transaction);

        LogDecision(resultTransaction);

        //prepara o evento de resultado para publicação, sem expor detalhes sensíveis da validação ou regras aplicadas
        var fraudFinalResult = new FraudEvaluatedResultEvent
        {
            TransactionId = resultTransaction.TransactionId,
            Status = resultTransaction.Decision.ToString(),
            RiskLevel = resultTransaction.RiskLevel.ToString(),
            CreatedAt = transaction.CreatedAt
        };

        var auditRepository = FraudAuditDocument.From(transaction, fraudFinalResult, resultTransaction.TriggeredRules);

        await _auditRepository.SaveAsync(auditRepository, context.CancellationToken);

        // Em produção, substituiríamos IPublishEndpoint por uma abstração IMessageBus
        // para encapsular a propagação do CorrelationId automaticamente em todos os publishes,
        // seguindo o mesmo padrão já adotado na API.
        await _publishEndpoint.Publish(fraudFinalResult, ctx =>
        {
            if (Guid.TryParse(_correlationContext.CorrelationId, out var guid))
                ctx.CorrelationId = guid;
        });
    }


    private void LogDecision(FraudEvaluationResult transaction)
    {
        if (transaction.Decision == Enums.FraudDecision.Rejected)
        {
            _logger.LogWarning("Transaction {TransactionId} rejected due to high risk. Risk Level: {RiskLevel}",
                transaction.TransactionId, transaction.RiskLevel);
        }
        else if (transaction.Decision == Enums.FraudDecision.Review)
        {
            _logger.LogInformation("Transaction {TransactionId} flagged for manual review. Risk Level: {RiskLevel}",
                transaction.TransactionId, transaction.RiskLevel);
        }
        else
        {
            _logger.LogInformation("Transaction {TransactionId} approved. Risk Level: {RiskLevel}",
                transaction.TransactionId, transaction.RiskLevel);
        }
    }
}
