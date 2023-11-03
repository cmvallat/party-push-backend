using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class AddHost
{
    public class Command : IRequest<string>
    {
        public Host Host { get; set; }
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
            return await _dbService.AddHost(request.Host);
        }
    }
}