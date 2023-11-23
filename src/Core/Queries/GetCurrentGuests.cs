using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GetCurrentGuests
{
    public class Query : IRequest<List<Guest>>
    {
        public string Party_code { get; set; }
    }

    public class Handler : IRequestHandler<Query, List<Guest>>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<List<Guest>> Handle(Query query, CancellationToken cancellationToken)
        {
            return await _dbService.GetCurrentGuests(query.Party_code);
        }
    }
}