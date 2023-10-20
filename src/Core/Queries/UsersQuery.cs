using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Queries;

public class UsersQuery
{
    public class Query : IRequest<User>
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Phone_Number { get; set; }
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
            return await _dbService.GetUser(query.Username, query.Password, query.Phone_Number);
        }
    }
}