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
        public int Invite_only { get; set; }
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
            //determine whether the party is open invite
            //Todo: perhaps handle on front end? i.e. short-circuit open-invite logic

            //if open to the public, no need to add a guest to an invite list
            if(request.Invite_only == 0)
            {
                return Common.Constants.Constants.OpenInviteMessage;
            }
            //if private party, add guest to invite list
            else if(request.Invite_only == 1)
            {
                return await _dbService.AddGuestFromHost(request.Guest_name, request.Party_code, request.Username);
            }
            else //hopefully unreachable code, this would mean something is wrong on the front end
            {
                return Common.Constants.Constants.InvalidInviteOnlyValueMessage;
            }

        }
    }
}