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
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;


namespace Api.PartyController
{
    [ApiController]
    [Route("[controller]")]

    public class PartyController : ControllerBase
    {
        private IMediator _mediator;
        //Todo: remove after testing
        private IPartyService _dbService;

        public PartyController(IMediator mediator, IPartyService dbService)
        {
            _mediator = mediator;
            //Todo: remove after testing
            _dbService = dbService;
        }

        //Refactor notes:
        //move logic to command
        //determine consistent spacing
        #region Host APIs

        //Create a party (add a Host object)
        //Todo: add more validation? phone number validation maybe?
        //Todo: potentially remove spotify_device_id if not needed or make non-required
        [HttpPost("add-host")]
        public async Task<IActionResult> AddHost(
            string Username,
            string Party_name, 
            string Party_code, 
            string Phone_number, 
            string Spotify_device_id, 
            int Invite_only)
        {
            List<String> paramsList = new List<String>(){
                Username,
                Party_name, 
                Party_code, 
                Phone_number, 
                Spotify_device_id, 
            };
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //create the Host that we want to upsert
            Models.Host host = new Models.Host
            {
                username = Username,
                party_name = Party_name,
                party_code = Party_code,
                phone_number = Phone_number,
                spotify_device_id = Spotify_device_id,
                invite_only = Invite_only
            };

            string result = await _mediator.Send(new AddHost.Command { Host = host });

            if(result == "Success!")
            {
                //on creation, text Host to confirm
                //functionality will be added later. commented out for now
                // string message = "Your party, " + Party_name + ", was successfully created!";
                // var returnedString = Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(Phone_number, message);
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Failed to insert Host into db" });
        }

        //Search for a particular party (Host) in database
        //Used only for internal purposes (other endpoints call it) so no need for password
        [HttpGet("get-host")]
        public async Task<IActionResult> GetHost(string party_code)
        {
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var host = await _mediator.Send(new GetHost.Query(){Party_code = party_code});

            if(host != null)
            {
                return StatusCode(200, new { message = host });
            }

            return StatusCode(500, new { message = "Failed to get Host from db" });
        }

        //Search for a particular party (Host) in database
        //Used for the host login page (hence needing the password)
        [HttpGet("get-host-from-check-in")]
        public async Task<IActionResult> GetHostFromCheckIn(
            string party_code, 
            string phone_number, 
            string password)
        {
            List<String> paramsList = new List<String>(){
                phone_number, 
                party_code, 
                password
            };
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var host = await _mediator.Send(new GetHostFromCheckIn.Query(){
                Party_code = party_code, 
                Phone_Number = phone_number, 
                Password = password
            });

            if(host != null)
            {
                return StatusCode(200, new { message = host });
            }

            return StatusCode(500, new { message = "Failed to get Host from db" });
        }
        #endregion

        #region Guest APIs

        //Adding a guest to a party from the host invitation flow
        //includes logic for open and closed invite parties 
        //Todo: move logic to command
        [HttpPost("add-guest-from-host")]
        public async Task<IActionResult> AddGuestFromHost(
            int host_invite_only, 
            string guest_name, 
            string party_code)
        {
            //Todo: need to remove host_invite_only? figure out what is needed here
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            string result = "Success!";
            
            //determine whether the party is open invite
            //Todo: perhaps handle on front end?
            //if open to the public, no need to add a guest to an invite list
            if(host_invite_only == 0)
            {
                return StatusCode(200, new {message = "Your party is open invite, no need to add guests! Just give them your party_code and tell them to check-in"});
            }

            //if private party, add guest to invite list
            else if(host_invite_only == 1)
            {
                result = await _mediator.Send(new AddGuestFromHost.Command { Guest_name = guest_name, Party_code = party_code });
            }
            
            else //hopefully unreachable code, this would mean something is wrong on the front end
            {
                return StatusCode(500, new { message = "Something went wrong with the invite_only parameter not being either 0 or 1." });
            }

            if(result == "Success!")
            {
                List<Guest> guest_list = await _mediator.Send(new GetAllGuests.Query() {Party_code = party_code});
                return StatusCode(200, new { message = guest_list });
            }

            return StatusCode(500, new { message = result });
        }

        //Adding a guest to a party from the guest check-in flow
        //includes logic for open and closed invite parties 
        [HttpPost("add-guest-from-check-in")]
        public async Task<IActionResult> AddGuestFromCheckIn(string party_code, string guest_name)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            string result = "Success!";

           //before we do anything, make sure the party exists
           Models.Host corresponding_host = await _mediator.Send(new GetHost.Query { Party_code = party_code });

           //if there is no party, throw error
           if(corresponding_host == null)
           {
                return StatusCode(500, new { message = "Couldn't find a party with this party code." });
           }

           if(corresponding_host.invite_only == 0) //if party exists and is open to the public
           {
                //get guest and see if they are currently at that party
                var invited_guest = await _mediator.Send(new GetGuest.Query() {Guest_name = guest_name, Party_code = party_code});

                //if they are not, let them in
                //for guests who have never joined (null) and joined previously but left (exist but at_party = 0)
                if(invited_guest == null || invited_guest.at_party == 0)
                {
                    Guest matching_guest = new Guest
                    {
                        guest_name = guest_name,
                        party_code = party_code,
                        at_party = 1
                    };
                    result = await _mediator.Send(new AddGuestFromCheckIn.Command { Guest = matching_guest});
                }

                //if they are already at the party, don't let them in (duplicate) and throw error
                else if(invited_guest.at_party == 1)
                {
                    return StatusCode(500, new { message = "There is already someone at this party with this name. Please try checking in again with a new name." });
                }
           }

           else if (corresponding_host.invite_only == 1) //otherwise, it is invite only and we have to check if they are invited
           {
                //query to check if they are invited
                var invited_guest = await _mediator.Send(new GetGuest.Query() {Guest_name = guest_name, Party_code = party_code});

                if(invited_guest != null) //if we found a record, that means they are invited
                {
                    if(invited_guest.at_party == 0) //if they aren't currently at the party, let them in
                    {
                        Guest new_guest_details = new Guest
                        {
                            guest_name = invited_guest.guest_name,
                            party_code = invited_guest.party_code,
                            at_party = 1
                        };
                        result = await _mediator.Send(new UpdateGuest.Command { Guest = new_guest_details });
                    }
                    else //if they are somehow at the party already (duplicate), throw error
                    {
                        return StatusCode(500, new { message = "There is already someone at this party with this name. Please try checking in again with a new name." });
                    }
                }
                else //if they are not invited, throw error
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
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Something went wrong." });
        }

        //Search for a particular guest at a particular party in database
        [HttpGet("get-guest")]
        public async Task<IActionResult> GetGuest(string guest_name, string party_code)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = "User could not be validated" });
            }
            else
            {
                var guest = await _mediator.Send(new GetGuest.Query() {
                    Guest_name = guest_name, 
                    Party_code = party_code, 
                    Username = UN});

                if(guest != null)
                {
                    return StatusCode(200, new { message = guest });
                }

                return StatusCode(500, new { message = "Failed to get Guest from db" });
            }
        }

         //Get the list of current guests at a particular party
        [HttpGet("get-current-guests")]
        public async Task<IActionResult> GetCurrentGuests(string party_code)
        {   
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //get party that we are checking the guest list for
            Models.Host corresponding_host = await _mediator.Send(new GetHost.Query() {Party_code = party_code});

            if(corresponding_host == null)
            {
                return StatusCode(500, new { message = "Could not get guest list, as that party does not exist" });
            }
            else
            {
                List<Guest> guest_list = await _mediator.Send(new GetCurrentGuests.Query() {Party_code = party_code});
                
                //if guest list is not empty, return the guest list
                if(guest_list != null)
                {
                    return StatusCode(200, new { guestList = guest_list });
                }
                else if(guest_list == null) //if it is empty, then there are no guests at that party
                {
                    return StatusCode(200, new { guestList = new List<Guest>(){} });
                }
            }
            
            //hopefully this is unreachable
            return StatusCode(500, new { message = "Something went wrong, failed to get Guest list from db" });
        }

        //Get the list of current guests at a particular party
        [HttpGet("get-all-guests")]
        public async Task<IActionResult> GetAllGuests(string party_code)
        {   
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //get party that we are checking the guest list for
            Models.Host corresponding_host = await _mediator.Send(new GetHost.Query() {Party_code = party_code});

            if(corresponding_host == null)
            {
                return StatusCode(500, new { message = "Could not get guest list, as that party does not exist" });
            }
            else
            {
                List<Guest> guest_list = await _mediator.Send(new GetAllGuests.Query() {Party_code = party_code});
                
                //if guest list is not empty, return the guest list
                if(guest_list != null)
                {
                    return StatusCode(200, new { guestList = guest_list });
                }
                else if(guest_list == null) //if it is empty, then there are no guests at that party
                {
                    return StatusCode(200, new { guestList = new List<Guest>(){} });
                }
            }
            
            //hopefully this is unreachable
            return StatusCode(500, new { message = "Something went wrong, failed to get Guest list from db" });
            
        }

        //Remove a guest permanently from the party (only host can do this)
        [HttpPost("delete-guest")]
        public async Task<IActionResult> DeleteGuest(string guest_name, string party_code)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var result = await _mediator.Send(new DeleteGuest.Command() {Party_code = party_code, Guest_name = guest_name});

            if(result == "Success!")
            {
                List<Guest> guest_list = await _mediator.Send(new GetAllGuests.Query() {Party_code = party_code});
                return StatusCode(200, new { message = guest_list });
            }

            return StatusCode(500, new { message = "Failed to delete Guest from db" });
        }

        //Guest leaves a party but can re-join later
        //Won't show up on current guest list
        [HttpPost("leave-party")]
        public async Task<IActionResult> GuestLeavesParty(string party_code, string guest_name)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
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
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Failed to remove guest from party in db" });
        }
        #endregion

        #region Both Host and Guest Endpoints

        //Search for all the Host and Guest objects in database with particular username
        [HttpGet("get-party-objects")]
        public async Task<IActionResult> GetPartyObjects(string username)
        {
            List<String> paramsList = new List<String>(){username};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var result = await _mediator.Send(new GetPartyObjects.Query { Username = username });

            if(result != null)
            {
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Failed to get Host and Guest objects from db" });
        }

        //End party (delete all guests and Host by party_code)
        [HttpPost("end-party")]
        public async Task<IActionResult> EndParty(string party_code)
        {
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

           var result = await _mediator.Send(new EndParty.Command { Party_code = party_code });

            if(result == "Success!")
            {
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Failed to get delete party from db" });
        }
        #endregion

        #region Food APIs

        //Adds a new row in Food table in DB representing a food item
        [HttpPost("add-food")]
        public async Task<IActionResult> AddFood(string party_code, string item_name)
        {
            List<String> paramsList = new List<String>(){item_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var result = await _mediator.Send(new AddFood.Command { Party_code = party_code, Item_name = item_name });

            if(result == "Success!")
            {
                List<Food> food_list = await _mediator.Send(new GetCurrentFoods.Query() {Party_code = party_code});
                return StatusCode(200, new { message = food_list });
            }

            return StatusCode(500, new { message = "Failed to add food item for party in db" });
        }

        //Get current list of Food items at a certain party
        [HttpPost("get-current-foods")]
        public async Task<IActionResult> GetCurrentFoods(string party_code)
        {
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //get party that the food is being added to
            //don't really need to do this unless we want to throw specific errors (which we do for now)
            Models.Host corresponding_host = await _mediator.Send(new GetHost.Query() {Party_code = party_code});

            if(corresponding_host == null)
            {
                return StatusCode(500, new { message = "Could not get food list, as that party does not exist" });
            }
            else
            {
                List<Food> food_list = await _mediator.Send(new GetCurrentFoods.Query() {Party_code = party_code});
                
                //if food list is not empty, return the food list
                if(food_list != null)
                {
                    return StatusCode(200, new { message = food_list });
                }
                else if(food_list == null) //if it is empty, then there are no food items at that party
                {
                    //Should display on initial management page loads (guest and host) and when no items
                    return StatusCode(200, new { message = "There are currently no food items at your party. Click the 'add food' button to add items available to guests." });
                }
            }
            
            //hopefully this is unreachable
            return StatusCode(500, new { message = "Something went wrong, failed to get Food list from db" });
        }

        //Updates the food status when a host changes status of an item and texts guests to inform them
        //Todo: add guest phone_number column and texting functionality
        [HttpPost("update-food-status-from-host")]
        public async Task<IActionResult> UpdateFoodStatusFromHost(string party_code, string status, string item_name)
        {
            List<String> paramsList = new List<String>(){item_name, party_code, status};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var result = "false";

            //valid status parameter - these are the only 3 valid statuses
            //Todo: eventually move to enum?
            if(status != "full" && status != "low" && status != "out")
            {
                return StatusCode(500, new { message = "Status was invalid b/c it was not 'full', 'low', or 'out' " });
            }

            //get party that the food is being altered at
            //don't really need to do this unless we want to throw specific errors (which we do for now)
            Models.Host corresponding_host = await _mediator.Send(new GetHost.Query() {Party_code = party_code});

            if(corresponding_host == null)
            {
                return StatusCode(500, new { message = "Could not change food status for this party, as party does not exist in database" });
            }
            else
            {
                //change status of food item in db
                result = await _mediator.Send(new UpdateFoodStatus.Command { Status = status, Item_name = item_name, Party_code = party_code });
            }

            if(result == "Success!")
            {
                //if successfully updated status, text the guests to let them know

                //Todo: Get list of all Guests at this party (where at_party = 1) and their phone_numbers
                //or better yet, create a guest phone number list when creating party and just update
                //every time a guest checks in

                string text = item_name + " has been reported as status " + status + " by host.";
                //foreach guest, text
                //Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(corresponding_host.phone_number, text);
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Failed to change food item status from host" });
        }

        //Texts host when a guest reports food as low or out and updates status of item
        //Could re-use this endpoint and pass in "from host" value to determine whether to text host or guests
        //but that would be confusing to handle on front end
        [HttpPost("update-food-status-from-guest")]
        public async Task<IActionResult> UpdateFoodStatusFromGuest(string party_code, string status, string guest_name, string item_name)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code, status, item_name};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //guest name should come from FE (guest management page)
            var result = "false";

            //valid status parameter - these are the only 3 valid statuses
            //Todo: eventually move to enum?
            if(status != "full" && status != "low" && status != "out")
            {
                return StatusCode(500, new { message = "Status was invalid b/c it was not 'full', 'low', or 'out' " });
            }
            
            //get party that the food is being altered at
            //don't really need to do this unless we want to throw specific errors (which we do for now)
            Models.Host corresponding_host = await _mediator.Send(new GetHost.Query() {Party_code = party_code});

            if(corresponding_host == null)
            {
                return StatusCode(500, new { message = "Could not report food status for this party, as party does not exist in database" });
            }
            else
            {
                //change status of food item in db
                result = await _mediator.Send(new UpdateFoodStatus.Command { Status = status, Item_name = item_name, Party_code = party_code });
            }

            if(result == "Success!")
            {
                //if successfully updated status, text the host to let them know
                // string text = guest_name + " reported " + item_name + " as " + status;
                // Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(corresponding_host.phone_number, text);
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = "Failed to report item status to host" });
        }

        //Removes a row representing a food item from the Food table in DB
        [HttpPost("delete-food")]
        public async Task<IActionResult> DeleteFood(string party_code, string item_name)
        {
            List<String> paramsList = new List<String>(){item_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var result = await _mediator.Send(new DeleteFood.Command { Party_code = party_code, Item_name = item_name });

            if(result == "Success!")
            {
                List<Food> food_list = await _mediator.Send(new GetCurrentFoods.Query() {Party_code = party_code});
                return StatusCode(200, new { message = food_list });
            }

            return StatusCode(500, new { message = "Failed to remove food item from party in db" });
        }

        #endregion

        #region User Authenication endpoints and methods

        //Adds a user to database
        [HttpGet("add-user")]
        public async Task<IActionResult> AddUser(string username, string password, string phone_number)
        {
            List<String> paramsList = new List<String>(){username, password, phone_number};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var result = await _mediator.Send(new AddUser.Command(){
                Username = username, 
                Password = password, 
                Phone_Number = phone_number
            });

            if(result == null)
            {
                return StatusCode(500, new { message = "Something went wrong, failed to add user to db" });
            }
            else if(result == "Success!")
            {
                return await GetJWT(username, password);
            }
            else
            {
                return StatusCode(500, new { message = result });
            }
        }

        //generates a JSON Web Token (JWT) for an existing and authenticated user
        [AllowAnonymous]
        [HttpPost("get-user")]
        public async Task<IActionResult> GetJWT(string username, string password)
        {
            List<String> paramsList = new List<String>(){username, password};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            var ValidatedUser = Authenticate(username, password).Result;
            if (ValidatedUser != null)
            {
                var token = GenerateToken(ValidatedUser);
                return StatusCode(200, new { message = token });
            }
            return StatusCode(404, new { message = "User not found" });
        }

        //Generates the actual JWT
        private string GenerateToken(ValidatedUser ValidatedUser)
        {
            //Todo: get from Secrets Manager, not hardcoded value

            // var key = _config["Jwt:Key"];
            // var issuer = _config["Jwt:Issuer"];
            // var audience = _config["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6xGrJLpenwmJXCIAlLPbdfbgctrbgvrtgcrtdbgvgrdffvxrsvfdfrcvftryr65gr4sdrger"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, ValidatedUser.Username),
                new Claim(ClaimTypes.Role, ValidatedUser.Role),
            };
            var token = new JwtSecurityToken(
                "https://localhost:5001/",
                "https://localhost:5001/",
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //"Authenticates" user by getting from database and assigning "validated" role
        private async Task<ValidatedUser> Authenticate(string username, string password)
        {
            var currentUser = await _mediator.Send(new GetUser.Query() {
                    Username = username,
                    Password = password,
                });

            if (currentUser != null)
            {
                return new ValidatedUser
                {
                    Username = currentUser.username,
                    Password = currentUser.password,
                    Role = "Validated",
                };
            }
            return null;
        }

        //validates and returns the username of a validated User 
        [HttpGet("validate-user")]
        [Authorize(Roles = "Validated")]
        public string GetValidatedUsername()
        {
            var currentUser = ValidateUser();
            return currentUser.Username;
        }
        private ValidatedUser ValidateUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                return new ValidatedUser
                {
                    Username = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                    Role = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value,
                };
            }
            return null;
        }
        
        #endregion
    }
}
