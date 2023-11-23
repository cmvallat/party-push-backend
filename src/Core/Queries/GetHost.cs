using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GetHost
{
    public class Query : IRequest<Host>
    {
        public string Party_code { get; set; }
    }

    public class Handler : IRequestHandler<Query, Host>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<Host> Handle(Query query, CancellationToken cancellationToken)
        {
            return await _dbService.GetHost(query.Party_code);
        }
    }
}