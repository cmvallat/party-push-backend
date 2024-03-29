using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class AddUser
{
    public class Command : IRequest<string>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Phone_Number { get; set; }
    }

    public class Handler : IRequestHandler<Command, string>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<string> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _dbService.AddUser(request.Username, request.Password, request.Phone_Number);
        }
    }
}