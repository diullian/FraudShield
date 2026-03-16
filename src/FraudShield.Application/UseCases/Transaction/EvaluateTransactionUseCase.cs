using FraudShield.Application.Messaging;
using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;
using FraudShield.Contracts.Events;
using FraudShield.Domain.Entities;
using FraudShield.Domain.Repositories.Transactions;
using MapsterMapper;

namespace FraudShield.Application.UseCases.Transaction;

public class EvaluateTransactionUseCase : IEvaluateTransactionUseCase
{

    private readonly ITransactionsWriteOnlyRepository _repository;
    private readonly IMessageBus _messageBus;
    private readonly IMapper _mapper;

    public EvaluateTransactionUseCase(ITransactionsWriteOnlyRepository repository, IMapper mapper, IMessageBus messageBus)
    {
        _repository = repository;
        _mapper = mapper;
        _messageBus = messageBus;
    }

    public async Task<ResponseEvaluateTransactionJson> ExecuteAsync(RequestEvaluateTransactionJson request, CancellationToken ct = default)
    {

        Validate(request);
        
        var transaction = _mapper.Map<FinancialTransaction>(request);
        
        await _repository.AddTransactionAsync(transaction);


        var transactionEvent = _mapper.Map<TransactionCreatedEvent>(transaction);
        await _messageBus.PublishAsync(transactionEvent, ct);


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
