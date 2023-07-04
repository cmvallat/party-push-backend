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

        //Create Party (Host object) endpoint
        [HttpPost("create-party")]

        // Todo: change from async to sync
        // Todo: add more validation for upserts??
        // Todo: potentially remove spotify_device_id if not needed or make non-required 

        public async Task<IActionResult> CreateParty([Required] string Party_name,[Required] string Party_code,[Required] string Phone_number, [Required] string Spotify_device_id,[Required] int Invite_only)
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

            Models.Host host = new Models.Host
            {
                party_name = Party_name,
                party_code = Party_code,
                phone_number = Phone_number,
                spotify_device_id = Spotify_device_id,
                invite_only = Invite_only
            };

            var result = await _mediator.Send(new CreateParty.Command { Host = host });

            if(result)
            {
                string message = "Your party, " + Party_name + ", was successfully created!";
                var returnedString = Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(Phone_number, message);
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to get upsert Host to db" });
        }

        //Add Guest from Host endpoint
        [HttpPost("add-guest-from-host")]

        // Todo: change from async to sync
        public async Task<IActionResult> AddGuestFromHost([Required] int host_invite_only, [Required] string guest_name, [Required] string party_code)
        {
            var result = false;
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
                throw new Exception("Something went wrong with the invite_only parameter not being either 0 or 1.");
            }

            if(result)
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Something went wrong adding guest from host." });
        }

         //Add Guest check-in endpoint
        [HttpPost("guest-check-in")]

        // Todo: change from async to sync
        public async Task<IActionResult> CheckInGuest([Required] string party_code, [Required] string guest_name)
        {
            bool result = false;
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
                throw new Exception("Couldn't find a party with this party code.");
           }
           //if party is open to the public, just go ahead and add guest (if they aren't already at party)
           if(corresponding_host.invite_only == 0)
           {
                var invited_guest = await _mediator.Send(new GuestQuery.Query() {Guest_name = guest_name, Party_code = party_code});
                if(invited_guest.at_party == 1)
                {
                    throw new Exception("There is already someone at this party with this name. Please try checking in again with a new name.");
                }
                else if(invited_guest.at_party == 0 || invited_guest == null) //if not currently at party
                {
                    Guest matching_guest = new Guest
                    {
                        guest_name = guest_name,
                        party_code = party_code,
                        at_party = 1
                    };
                    result = await _mediator.Send(new AddGuestFromCheckIn.Command { Guest = matching_guest});
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
                        throw new Exception("There is already someone at this party with this name. Please try checking in again with a new name.");
                    }
                }
                else //they are not invited
                {
                    throw new Exception("Oops, this is awkward! Doesn't seem like you're on the invite list. Please check that you spelled your name and the party code right, or try joining another party.");
                }
           }
           else //hopefully unreachable code because this would mean something wrong with FE or invite_only (DB error)
           {
            throw new Exception("Something went wrong with guest check in, invite_only being neither 0 or 1.");
           }

            if(result)
            {
                return Ok(result);
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
        public async Task<IActionResult> DeleteGuestByNameAndCode([Required] string Guest_name, [Required] string Party_code, [Required] int At_party)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(Party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(Guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }

            Guest guest = new Guest
            {
                guest_name = Guest_name,
                party_code = Party_code,
                at_party = At_party
            };

            var result = await _mediator.Send(new DeleteGuest.Command() {Guest = guest});

            if(result)
            {
                return Ok(result);
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

            List<Guest> guest_list = await _mediator.Send(new CurrentGuestsQuery.Query() {Party_code = party_code});
            
            //if guest list is not empty, return the guest list
            if(guest_list != null)
            {
                return Ok(guest_list);
            }
            else if(guest_list == null) //if it is empty, throw exception
            {
                //Todo: eventually move to handle on FE instead of exception!!!
                throw new Exception("There are currently no guests at your party");
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

            if(result)
            {
                return Ok(result);
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

            if(result)
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }

        // un-comment this function when EC2 is up so we can test secret - only works on EC2, not locally
        // static async Task<string> GetSecret()
        // {
        //     string secretName = "rds-party-db-secret";
        //     string region = "us-west-1";

        //     //can only get it without specifying values when running on EC2 instance, not locally
        //     //IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        //     GetSecretValueRequest request = new GetSecretValueRequest
        //     {
        //         SecretId = secretName,
        //         VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        //     };

        //     GetSecretValueResponse response;

        //     try
        //     {
        //         response = await client.GetSecretValueAsync(request);
        //     }
        //     catch (Exception e)
        //     {
        //         // For a list of the exceptions thrown, see
        //         // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
        //         throw e;
        //     }
        //
        //     for formatting the secret into correct db connecion string format:
        //     string secretString = response.SecretString;
        //     // JObject secretObject = JObject.Parse(secretString);
        //     // string username = (string)secretObject["username"];
        //     // string host = (string)secretObject["host"];
        //     // int port = (int)secretObject["port"];

        //     return response.SecretString;
        // }
        
    }
}
