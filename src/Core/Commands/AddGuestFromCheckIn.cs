using Mediator;
using Models;
using DataLayer;
using System.ComponentModel.DataAnnotations;
using Common;

namespace Core.Commands;

public class AddGuestFromCheckIn
{
    public class Command : IRequest<string>
    {
        // public string Guest_name { get; set; }
        // public string Party_code { get; set; }
        // public int Username { get; set; }
        public Models.Host Host { get; set; }
        public Models.Guest Guest { get; set; }
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
            var corresponding_host = request.Host;
            var invited_guest = request.Guest;
            //if there is no party, throw error
            if(corresponding_host == null)
            {
                return Common.Constants.Constants.CouldntFindHostMessage;
            }

            //if party exists and is open to the public 
            if(corresponding_host.invite_only == 0) 
            {
                //if guest is not at the party, let them in
                //for guests who have never joined (null) and joined previously but left (exist but at_party = 0)
                if(invited_guest == null || invited_guest.at_party == 0)
                {
                    return await _dbService.AddGuestFromCheckIn(
                        invited_guest.username, 
                        invited_guest.guest_name, 
                        invited_guest.party_code, 
                        1
                    );
                }

                //if they are already at the party (duplicate entry), don't let them in and throw error
                else if(invited_guest.at_party == 1)
                {
                    return Common.Constants.Constants.GuestAlreadyAtPartyMessage;
                }
           }
            //otherwise, it is invite-only and we have to check if they are invited
            else if (corresponding_host.invite_only == 1)
            {
                //if we were passed a valid record, that means they are invited
                if(invited_guest != null)
                {
                    //if they aren't currently at the party, let them in
                    if(invited_guest.at_party == 0)
                    {
                        return Common.Constants.Constants.OkToUpdateGuestMessage;
                    }
                    //if they are currently at the party, throw exception
                    else
                    {
                        return Common.Constants.Constants.GuestAlreadyAtPartyMessage;
                    }
                }
                //if they are not invited, throw exception (deny entry)
                else
                {
                    return Common.Constants.Constants.NotInvitedMessage;
                }
            }
            //hopefully unreachable code because this would mean something wrong with FE or invite_only (DB error)
            else
            {
                return Common.Constants.Constants.InvalidInviteOnlyValueMessage;
            }
            //default code path return
            return Common.Constants.Constants.FailedToInsertMessage;
        }
    }
}