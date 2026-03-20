using FraudShield.Application.Exceptions;
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

    private readonly ITransactionsWriteOnlyRepository _writeOnlyRepository;
    private readonly ITransactionsReadOnlyRepository _readOnlyRepository;
    private readonly IMessageBus _messageBus;
    private readonly IMapper _mapper;
    private readonly ICorrelationContext _correlation;
    public EvaluateTransactionUseCase(ITransactionsWriteOnlyRepository repository, IMapper mapper, IMessageBus messageBus, ICorrelationContext correlation, ITransactionsReadOnlyRepository readOnlyRepository)
    {
        _writeOnlyRepository = repository;
        _mapper = mapper;
        _messageBus = messageBus;
        _correlation = correlation;
        _readOnlyRepository = readOnlyRepository;
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
        
        var existsCorrelation= await _readOnlyRepository.ExistsByCorrelationId(transaction.CorrelationId, ct);
        if (existsCorrelation)
        {
            throw new ConflictException($"A transaction with the same correlation ID already exists. Id: {transaction.CorrelationId}");
        }

        var transactionId = await _writeOnlyRepository.AddTransactionAsync(transaction);
        
        //publica o evento de transação criada para o barramento de mensagens
        var transactionEvent = _mapper.Map<TransactionCreatedEvent>(transaction);
        transactionEvent.TransactionId = transactionId; //associa id

        await _messageBus.PublishAsync(transactionEvent, ct);

        //retorna a resposta para o cliente
        return _mapper.Map<ResponseEvaluateTransactionJson>(transaction);
    }

    private void Validate(RequestEvaluateTransactionJson request)
    {
        var errors = new List<string>();

        if (request == null)
            throw new ValidationException("Request is required.");

        if (request.Amount <= 0)
            errors.Add("Amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Customer.Document))
            errors.Add("Customer Document is required.");

        if (string.IsNullOrWhiteSpace(request.Merchant.Name))
            errors.Add("Merchant Name is required.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
