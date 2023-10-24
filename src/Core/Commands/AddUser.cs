using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class AddUser
{
    public class Command : IRequest<string>
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
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