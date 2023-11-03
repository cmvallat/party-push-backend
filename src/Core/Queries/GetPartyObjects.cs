using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GetPartyObjects
{
    public class Query : IRequest<List<object>>
    {
        public string Username{ get; set; }
    }

    public class Handler : IRequestHandler<Query, List<object>>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<List<object>> Handle(Query query, CancellationToken cancellationToken)
        {
            List<Models.Host> hosts = await _dbService.GetHostsFromUser(query.Username);
            List<Models.Guest> guests = await _dbService.GetGuestsFromUser(query.Username);
            return new List<object>(){ hosts, guests };
        }
    }
}