using FraudShield.Application.Interfaces;
using FraudShield.Application.Messaging;
using FraudShield.Communication.Contracts;
using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;
using FraudShield.Domain.Entities;
using FraudShield.Domain.Repositories.Transactions;
using MapsterMapper;

namespace FraudShield.Application.UseCases.Transaction.Register;

public class EvaluateTransactionUseCase : IEvaluateTransactionUseCase
{

    private readonly ITransactionsWriteOnlyRepository _repository;
    private readonly IMessageBus _messageBus;
    private readonly IMapper _mapper;
    private readonly ICorrelationContext _correlation;
    public EvaluateTransactionUseCase(ITransactionsWriteOnlyRepository repository, IMapper mapper, IMessageBus messageBus, ICorrelationContext correlation)
    {
        _repository = repository;
        _mapper = mapper;
        _messageBus = messageBus;
        _correlation = correlation;
    }

    public async Task<ResponseEvaluateTransactionJson> ExecuteAsync(
        RequestEvaluateTransactionJson request,
        string idempotencyKey,
        CancellationToken ct = default)
    {
        Validate(request);
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

        //salva a transação no banco de dados
        var transaction = _mapper.Map<FinancialTransaction>(request);
        transaction.IdempotencyKey = idempotencyKey;

        // atribui CorrelationId
        transaction.CorrelationId = Guid.TryParse(_correlation.CorrelationId, out var guid)
            ? guid
            : Guid.NewGuid();

        var transactionId = await _repository.AddTransactionAsync(transaction);
        
        //publica o evento de transação criada para o barramento de mensagens
        var transactionEvent = _mapper.Map<TransactionCreatedEvent>(transaction);
        transactionEvent.TransactionId = transactionId; //associa id

        await _messageBus.PublishAsync(transactionEvent, ct);

        //retorna a resposta para o cliente
        return _mapper.Map<ResponseEvaluateTransactionJson>(transaction);
    }

    private void Validate(RequestEvaluateTransactionJson request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));
        if (string.IsNullOrWhiteSpace(request.Customer.Document))
            throw new ArgumentException("Customer Document is required.", nameof(request.Customer.Document));
        if (string.IsNullOrWhiteSpace(request.Merchant.Name))
            throw new ArgumentException("Merchant Name is required.", nameof(request.Merchant.Name));
    }
}
