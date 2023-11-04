using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;

namespace Core.Commands;

public class AddHost
{
    public class Command : IRequest<string>
    {
        public string username { get; set; }
        public string party_name { get; set; }
        public string party_code { get; set; }
        public string phone_number { get; set; }
        //Todo: add when implementing spotify feature
        //public string spotify_device_id { get; set; }
        public int invite_only { get; set; }
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
            return await _dbService.AddHost(
                request.username,
                request.party_name,
                request.party_code,
                request.phone_number,
                //request.spotify_device_id,
                request.invite_only
            );
        }
    }
}