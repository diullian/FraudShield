using FraudShield.Communication.Contracts;
using FraudShield.Domain.Repositories.Transactions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FraudShield.Infrastructure.Messaging.Consumers;

public class FraudResultConsumer : IConsumer<FraudEvaluatedResultEvent>
{
    private readonly ITransactionsUpdateOnlyRepository _transactionsUpdateOnlyRepository;
    private readonly ILogger<FraudResultConsumer> _logger;
    public FraudResultConsumer(ITransactionsUpdateOnlyRepository transactionUpdateRepository, ILogger<FraudResultConsumer> logger)
    {
        _transactionsUpdateOnlyRepository = transactionUpdateRepository;   
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FraudEvaluatedResultEvent> context)
    {

        _logger.LogInformation("----> Iniciando Consume do transact.id : {TransactionId}", context.Message.TransactionId);
        await Task.Delay(TimeSpan.FromSeconds(10)); // Simula um processamento mais demorado
        _logger.LogInformation("----> Após 10 segundos, segue o fluxo");


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
