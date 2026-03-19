using FraudShield.Application.Interfaces;
using FraudShield.Communication.Contracts;
using FraudShield.Domain.Repositories.Transactions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FraudShield.Infrastructure.Messaging.Consumers;

public class FraudResultConsumer : IConsumer<FraudEvaluatedResultEvent>
{
    private readonly ITransactionsUpdateOnlyRepository _transactionsUpdateOnlyRepository;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogger<FraudResultConsumer> _logger;
    public FraudResultConsumer(ITransactionsUpdateOnlyRepository transactionUpdateRepository, ILogger<FraudResultConsumer> logger, ICorrelationContext correlationContext)
    {
        _transactionsUpdateOnlyRepository = transactionUpdateRepository;   
        _logger = logger;
        _correlationContext = correlationContext;
    }

    public async Task Consume(ConsumeContext<FraudEvaluatedResultEvent> context)
    {
        var correlationId = context.CorrelationId?.ToString() ?? "N/A";


        _logger.LogInformation("[*** RESULTADO - Consume API ***]");
        _logger.LogInformation("Iniciando Consume do transact.id : {TransactionId}  <> CorrelationId: {CorrelationId}", context.Message.TransactionId, correlationId);
        await Task.Delay(TimeSpan.FromSeconds(10)); // Simula um processamento mais demorado
        _logger.LogInformation("");


        if (!Enum.TryParse< Domain.Enums.TransactionStatus> (context.Message.Status, out var status))
        {
            _logger.LogWarning(
                "Invalid status received: {Status} for transaction {TransactionId}",
                context.Message.Status,
                context.Message.TransactionId);
            return; // descarta a mensagem
        }

        if (!Enum.TryParse<Domain.Enums.RiskLevel>(context.Message.RiskLevel, out var riskLevel))
        {
            _logger.LogWarning(
                "Invalid risk level received: {RiskLevel} for transaction {TransactionId}",
                context.Message.RiskLevel,
                context.Message.TransactionId);
            return; // descarta a mensagem
        }


        _logger.LogInformation("**Updating transaction** {TransactionId} with status {Status} and risk level {RiskLevel} at {ProcessedAt}",
            context.Message.TransactionId,
            context.Message.Status,
            context.Message.RiskLevel,
            context.Message.ProcessedAt );

        await _transactionsUpdateOnlyRepository.UpdateStatusAsync(
            context.Message.TransactionId,
            status,
            riskLevel,
            context.Message.ProcessedAt);

    }
}
