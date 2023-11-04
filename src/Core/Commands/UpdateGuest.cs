using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class UpdateGuest
{
    public class Command : IRequest<string>
    {
        public string Party_code { get; set; }
        public int At_party { get; set; }
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
            return await _dbService.UpdateGuest(request.Party_code, request.At_party, request.Username);
        }
    }
}