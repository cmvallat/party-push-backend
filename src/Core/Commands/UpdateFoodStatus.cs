using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class UpdateFoodStatus
{
    public class Command : IRequest<string>
    {
        public string Status { get; set; }
        public string Item_name { get; set; }
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
            return await _dbService.UpdateFoodStatus(request.Party_code, request.Status, request.Item_name);
        }
    }
}