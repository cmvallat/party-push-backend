using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GetPartyInfo
{
    public class Query : IRequest<List<object>>
    {
        public string Party_code{ get; set; }
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
            //get guests and food
            List<Models.Guest> guests = await _dbService.GetAllGuests(query.Party_code);
            List<Models.Food> foods = await _dbService.GetCurrentFoods(query.Party_code);
            return new List<object>(){ guests, foods };
        }
    }
}