using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GuestQuery
{
    public class Query : IRequest<Guest>
    {
        [Required]
        public string Party_code { get; set; }

        [Required]
        public string Guest_name { get; set; }

        [Required]
        public string Username { get; set; }
    }

    public class Handler : IRequestHandler<Query, Guest>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<Guest> Handle(Query query, CancellationToken cancellationToken)
        {
            return await _dbService.GetGuest(query.Party_code, query.Guest_name, query.Username);
        }
    }
}