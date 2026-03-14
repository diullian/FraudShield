using FraudShield.Application.Mapping;
using FraudShield.Communication.Enums;
using FraudShield.Communication.Requests;
using FraudShield.Communication.Responses;
using FraudShield.Domain.Entities;
using FraudShield.Domain.Repositories.Transactions;
using MapsterMapper;

namespace FraudShield.Application.UseCases.Transaction;

public class EvaluateTransactionUseCase : IEvaluateTransactionUseCase
{

    private readonly ITransactionsWriteOnlyRepository _repository;
    private readonly IMapper _mapper;

    public EvaluateTransactionUseCase(ITransactionsWriteOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseEvaluateTransactionJson> ExecuteAsync(RequestEvaluateTransactionJson request)
    {

        Validate(request);
        
        var transaction = _mapper.Map<FinancialTransaction>(request);
        
        await _repository.AddTransactionAsync(transaction);


        return _mapper.Map<ResponseEvaluateTransactionJson>(transaction);
        //return Task.FromResult(new ResponseEvaluateTransactionJson
        //{
        //    TransactionId = transaction.Id,
        //    Status = TransactionStatus.Pending,
        //    Message = "Transaction received and being processed. Waiting please!!",
        //});

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
