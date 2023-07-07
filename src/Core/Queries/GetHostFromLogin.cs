using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class HostFromCheckInQuery
{
    public class Query : IRequest<Host>
    {
        [Required]
        public string Party_code { get; set; }
        [Required]
        public string Phone_Number { get; set; }
        [Required]
        public string Password { get; set; }
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
            return await _dbService.GetHostFromCheckIn(query.Party_code, query.Phone_Number, query.Password);
        }
    }
}