using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Amazon;
using Amazon.Runtime;
using Amazon.RDSDataService;
using Amazon.RDSDataService.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Models;
using Mediator;
using Core.Queries;
using Core.Commands;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Common;

namespace Api.DemoController
{
    [ApiController]
    [Route("[controller]")]

    public class DemoController : ControllerBase
    {
        private IMediator _mediator; 

        public DemoController(IMediator mediator)
        {
            _mediator = mediator;
        }

         //Send SMS endpoint
        [HttpPost("send-sms-test")]

        // Todo: move logic to certain endpoints, not its own
        public async Task<IActionResult> SendSMS([Required] string phoneNum, [Required] string messageBody)
        {
            string result = Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(phoneNum, messageBody);
            return StatusCode(200, new { message = result });
        }

        #region Check-In APIs
        //Create Party (Host object) endpoint
        [HttpPost("create-party")]

        // Todo: change from async to sync
        // Todo: add more validation for upserts??
        // Todo: potentially remove spotify_device_id if not needed or make non-required 

        public async Task<IActionResult> CreateParty([Required] string Party_name,[Required] string Party_code,[Required] string Phone_number, [Required] string Spotify_device_id,[Required] int Invite_only, [Required] string Password)
        {
            //validate input - make sure they passed a value for party_code, party_name and phone_number
            if(String.IsNullOrWhiteSpace(Party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(Party_name))
            {
                return StatusCode(500, new { message = "Party Name was invalid" });
            }

            if(String.IsNullOrWhiteSpace(Phone_number))
            {
                return StatusCode(500, new { message = "Phone Number was invalid" });
            }

            if(String.IsNullOrWhiteSpace(Password))
            {
                return StatusCode(500, new { message = "Phone Number was invalid" });
            }

            Models.Host host = new Models.Host
            {
                party_name = Party_name,
                party_code = Party_code,
                phone_number = Phone_number,
                spotify_device_id = Spotify_device_id,
                invite_only = Invite_only,
                password = Password
            };

            string result = await _mediator.Send(new CreateParty.Command { Host = host });

            if(result == "Success!")
            {
                string message = "Your party, " + Party_name + ", was successfully created!";
                var returnedString = Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(Phone_number, message);
                return Ok();
            }

            return StatusCode(500, new { message = "Failed to get upsert Host to db" });
        }

        //Add Guest from Host endpoint
        [HttpPost("add-guest-from-host")]

        // Todo: change from async to sync
        public async Task<IActionResult> AddGuestFromHost([Required] int host_invite_only, [Required] string guest_name, [Required] string party_code)
        {
            string result = "Success!";
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }

            //determine whether the party is open invite

            //if open to the public, no need to add a guest to an invite list
            //perhaps handle on front end?
            if(host_invite_only == 0)
            {
                return StatusCode(200, new {message = "Your party is open invite, no need to add guests! Just give them your party_code and tell them to check-in"});
            }
            else if(host_invite_only == 1) //if private party, add guest to invite list
            {
                result = await _mediator.Send(new AddGuestFromHost.Command { Guest_name = guest_name, Party_code = party_code });
            }
            else //hopefully unreachable code, this would mean something is wrong on the front end
            {
                return StatusCode(500, new { message = "Something went wrong with the invite_only parameter not being either 0 or 1." });
            }

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = result });
        }

         //Add Guest check-in endpoint
        [HttpPost("guest-check-in")]

        // Todo: change from async to sync
        public async Task<IActionResult> CheckInGuest([Required] string party_code, [Required] string guest_name)
        {
            string result = "Success!";
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }

           Models.Host corresponding_host = await _mediator.Send(new HostQuery.Query { Party_code = party_code });
           if(corresponding_host == null)
           {
                return StatusCode(500, new { message = "Couldn't find a party with this party code." });
           }
           //if party is open to the public
           if(corresponding_host.invite_only == 0)
           {
                //get guest and see if they are currently at that party
                var invited_guest = await _mediator.Send(new GuestQuery.Query() {Guest_name = guest_name, Party_code = party_code});
                //if they are not, let them in

                if(invited_guest == null || invited_guest.at_party == 0) //if not currently at party
                {
                    Guest matching_guest = new Guest
                    {
                        guest_name = guest_name,
                        party_code = party_code,
                        at_party = 1
                    };
                    result = await _mediator.Send(new AddGuestFromCheckIn.Command { Guest = matching_guest});
                }

                //if they are, don't let them in (duplicate)
                else if(invited_guest.at_party == 1)
                {
                    return StatusCode(500, new { message = "There is already someone at this party with this name. Please try checking in again with a new name." });
                }
                
           }
           else if (corresponding_host.invite_only == 1)//otherwise, it is invite only and we have to check if they are invited
           {
                //query to check if they are invited
                var invited_guest = await _mediator.Send(new GuestQuery.Query() {Guest_name = guest_name, Party_code = party_code});
                if(invited_guest != null) //then they are invited
                {
                    if(invited_guest.at_party == 0) //if they aren't currently at the party, let them in
                    {
                        Guest new_guest_details = new Guest
                        {
                            guest_name = invited_guest.guest_name,
                            party_code = invited_guest.party_code,
                            at_party = 1
                        };
                        //Todo: add new command to updateGuest
                        result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });
                    }
                    else //they are somehow at the party already, throw error
                    {
                        return StatusCode(500, new { message = "There is already someone at this party with this name. Please try checking in again with a new name." });
                    }
                }
                else //they are not invited
                {
                    return StatusCode(500, new { message = "Oops, this is awkward! Doesn't seem like you're on the invite list. Please check that you spelled your name and the party code right, or try joining another party." });
                }
           }
           else //hopefully unreachable code because this would mean something wrong with FE or invite_only (DB error)
           {
            return StatusCode(500, new { message = "Something went wrong with guest check in, invite_only being neither 0 or 1." });
           }

            if(result == "Success!")
            {
                return Ok();
            }

            return StatusCode(500, new { message = "Something went wrong." });
        }

        //Get Host endpoint
        [HttpGet("get-host")]

        // Todo: change from async to sync
        public async Task<IActionResult> GetHostByPartyCode([Required] string party_code)
        {
            //validate input - make sure they passed a value for party_code
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            var host = await _mediator.Send(new HostQuery.Query() {Party_code = party_code});

            if(host != null)
            {
                return Ok(host);
            }

            return StatusCode(500, new { message = "Failed to get Host from db" });
        }

        //Get Host endpoint
        [HttpGet("get-host-from-check-in")]

        // Todo: change from async to sync
        public async Task<IActionResult> GetHostFromCheckIn([Required] string party_code, [Required] string phone_number, [Required] string password)
        {
            //validate input - make sure they passed a value for party_code
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(phone_number))
            {
                return StatusCode(500, new { message = "Phone Number was invalid" });
            }
            if(String.IsNullOrWhiteSpace(password))
            {
                return StatusCode(500, new { message = "Password was invalid" });
            }

            var host = await _mediator.Send(new HostFromCheckInQuery.Query() {Party_code = party_code, Phone_Number = phone_number, Password = password});

            if(host != null)
            {
                return Ok(host);
            }

            return StatusCode(500, new { message = "Failed to get Host from db" });
        }

        //Get Guest endpoint
        [HttpGet("get-guest")]

        // Todo: change from async to sync
        public async Task<IActionResult> GetGuestByNameAndCode([Required] string guest_name, [Required] string party_code)
        {
            //validate input - make sure they passed a value for party_code
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            
            //validate input - make sure they passed a value for guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            var guest = await _mediator.Send(new GuestQuery.Query() {Guest_name = guest_name, Party_code = party_code});

            if(guest != null)
            {
                return Ok(guest);
            }

            return StatusCode(500, new { message = "Failed to get Guest from db" });
        }

         //Delete Guest endpoint
        [HttpPost("delete-guest")]

        // Todo: change from async to sync
        public async Task<IActionResult> DeleteGuestByNameAndCode([Required] string guest_name, [Required] string party_code)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }

            var result = await _mediator.Send(new DeleteGuest.Command() {Party_code = party_code, Guest_name = guest_name});

            if(result == "Success!")
            {
                return Ok();
            }

            return StatusCode(500, new { message = "Failed to delete Guest from db" });
        }

         //Get Current Guests endpoint
        [HttpGet("get-current-guest-list")]

        // Todo: change from async to sync
        public async Task<IActionResult> GetCurrentGuestListByCode([Required] string party_code)
        {   
            //validate input - make sure they passed a value for guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            //get party of the guests - don't really need to do this unless we want to throw specific errors (which for now I have decided we do)
            Models.Host corresponding_host = await _mediator.Send(new HostQuery.Query() {Party_code = party_code});
            if(corresponding_host == null)
            {
                return StatusCode(500, new { message = "Could not get guest list, as that party does not exist" });
            }
            else
            {
                List<Guest> guest_list = await _mediator.Send(new CurrentGuestsQuery.Query() {Party_code = party_code});
                
                //if guest list is not empty, return the guest list
                if(guest_list != null)
                {
                    return Ok(guest_list);
                }
                else if(guest_list == null) //if it is empty, then there are no guests at that party
                {
                    //Todo: eventually move to handle on FE instead of exception!!!
                    return StatusCode(200, new { message = "There are currently no guests at your party" });
                }
            }
            
            //hopefully this is unreachable
            return StatusCode(500, new { message = "Something went wrong, failed to get Guest list from db" });
            
        }

        //End party (delete all guests and host) endpoint
        [HttpPost("end-party")]

        // Todo: change from async to sync
        public async Task<IActionResult> EndParty([Required] string party_code)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

           var result = await _mediator.Send(new EndParty.Command { Party_code = party_code });

            if(result == "Success!")
            {
                return Ok();
            }

            return StatusCode(500, new { message = "Failed to get delete party from db" });
        }

         //End party (delete all guests and host) endpoint
        [HttpPost("leave-party")]

        // Todo: change from async to sync
        public async Task<IActionResult> GuestLeavesParty([Required] string party_code, [Required] string guest_name)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            Guest new_guest_details = new Guest
            {
                guest_name = guest_name,
                party_code = party_code,
                at_party = 0
            };
            var result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

        #endregion

        #region Refreshment APIs
           //End party (delete all guests and host) endpoint
        [HttpPost("add-food-item-from-host")]

        // Todo: change from async to sync
        public async Task<IActionResult> AddFoodItemFromHost([Required] string party_code, [Required] string guest_name)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            Guest new_guest_details = new Guest
            {
                guest_name = guest_name,
                party_code = party_code,
                at_party = 0
            };
            var result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

        //End party (delete all guests and host) endpoint
        [HttpPost("remove-food-item-from-host")]

        // Todo: change from async to sync
        public async Task<IActionResult> RemoveFoodItemFromHost([Required] string party_code, [Required] string guest_name)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            Guest new_guest_details = new Guest
            {
                guest_name = guest_name,
                party_code = party_code,
                at_party = 0
            };
            var result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

           //End party (delete all guests and host) endpoint
        [HttpPost("change-food-status-from-host")]

        // Todo: change from async to sync
        public async Task<IActionResult> ChangeFoodStatusFromHost([Required] string party_code, [Required] string guest_name)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            Guest new_guest_details = new Guest
            {
                guest_name = guest_name,
                party_code = party_code,
                at_party = 0
            };
            var result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

           //End party (delete all guests and host) endpoint
        [HttpPost("report-food-status-from-guest")]

        // Todo: change from async to sync
        public async Task<IActionResult> ReportFoodStatusFromGuest([Required] string party_code, [Required] string guest_name)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            Guest new_guest_details = new Guest
            {
                guest_name = guest_name,
                party_code = party_code,
                at_party = 0
            };
            var result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

           //End party (delete all guests and host) endpoint
        [HttpPost("get-current-food-list")]

        // Todo: change from async to sync
        public async Task<IActionResult> GetCurrentFoodList([Required] string party_code, [Required] string guest_name)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }
            if(String.IsNullOrWhiteSpace(guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }
            Guest new_guest_details = new Guest
            {
                guest_name = guest_name,
                party_code = party_code,
                at_party = 0
            };
            var result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });

            if(result == "Success!")
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

        #endregion
    }
}
