using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class RemoveFoodItem
{
    public class Command : IRequest<string>
    {
        // [Required]
        // public string Guest_name { get; set; }
        [Required]
        public string Party_code { get; set; }
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
            return await _dbService.RemoveFoodItem(request.Party_code);
        }
    }
}