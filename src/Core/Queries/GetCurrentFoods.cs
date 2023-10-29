using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GetCurrentFoods
{
    public class Query : IRequest<List<Food>>
    {
        [Required]
        public string Party_code { get; set; }
    }

    public class Handler : IRequestHandler<Query, List<Food>>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<List<Food>> Handle(Query query, CancellationToken cancellationToken)
        {
            return await _dbService.GetCurrentFoods(query.Party_code);
        }
    }
}