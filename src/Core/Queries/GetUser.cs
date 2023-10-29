using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class GetUser
{
    public class Query : IRequest<User>
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class Handler : IRequestHandler<Query, User>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<User> Handle(Query query, CancellationToken cancellationToken)
        {
            return await _dbService.GetUser(query.Username, query.Password);
        }
    }
}