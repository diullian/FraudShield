using FraudShield.Communication.Responses;
using FraudShield.Domain.Repositories.Transactions;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FraudShield.Application.UseCases.Transaction.GetById;

public class GetTransactionsByIdUseCase : IGetTransactionsByIdUseCase
{
    private readonly ITransactionsReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetTransactionsByIdUseCase(ITransactionsReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseShortTransactionJson> Execute(Guid transactionId, CancellationToken ct = default)
    {
        var result = await _repository.GetById(transactionId, ct);
        if(result is null)
        {
            throw new Exception($"Transaction with id {transactionId} not found.");
        }

        return _mapper.Map<ResponseShortTransactionJson>(result);
    }
}
