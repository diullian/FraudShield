using FraudShield.Communication.Responses;
using FraudShield.Domain.Repositories.Transactions;
using MapsterMapper;

namespace FraudShield.Application.UseCases.Transaction.GetAll;

public class GetAllTransactionsUseCase : IGetAllTransactionsUseCase
{
    private readonly ITransactionsReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetAllTransactionsUseCase(ITransactionsReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<ResponseTransactionsJson> Execute(CancellationToken ct = default)
    {
        var result = await _repository.GetAll();

        return new ResponseTransactionsJson
        {
            Transactions = _mapper.Map<List<ResponseShortTransactionJson>>(result)
        };
    }
}
