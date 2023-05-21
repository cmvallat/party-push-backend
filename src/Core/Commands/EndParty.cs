using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class EndParty
{
    public class Command : IRequest<bool>
    {
        [Required]
        public string Party_code { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IPartyService _dbService;

        public Handler(IPartyService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async ValueTask<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _dbService.EndParty(request.Party_code);
        }
    }
}