using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class AddGuestFromHost
{
    public class Command : IRequest<string>
    {
        public string Guest_name { get; set; }
        public string Party_code { get; set; }
        public string Username { get; set; }
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
            return await _dbService.AddGuestFromHost(request.Guest_name, request.Party_code, request.Username);
        }
    }
}