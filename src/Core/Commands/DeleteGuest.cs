using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class DeleteGuest
{
    public class Command : IRequest<string>
    {
        [Required]
        public string Party_code { get; set; }
        [Required]
        public string Guest_name { get; set; }
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
            return await _dbService.DeleteGuest(request.Party_code, request.Guest_name);
        }
    }
}