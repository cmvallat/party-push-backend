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
        public PartyController(IMediator mediator, IPartyService dbService)
        {
            _mediator = mediator;
        }

        //Refactor notes:
        //determine consistent spacing
        #region Host APIs

        //Create a party (add a Host object)
        //Todo: add more validation? phone number validation maybe?
        //Todo: add spotify_device_id back in when implementing Spotify feature
        [HttpPost("add-host")]
        public async Task<IActionResult> AddHost(
            string Party_name, 
            string Party_code, 
            string Phone_number, 
            //string Spotify_device_id, 
            int Invite_only)
        {
            List<String> paramsList = new List<String>(){
                Party_name, 
                Party_code, 
                Phone_number, 
                //Spotify_device_id, 
            };
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //make sure anyone trying to create a party is a validated user
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }

            string result = await _mediator.Send(new AddHost.Command {
                username = UN,
                party_name = Party_name,
                party_code = Party_code,
                phone_number = Phone_number,
                //spotify_device_id = Spotify_device_id,
                invite_only = Invite_only 
            });

            if(result == Common.Constants.Constants.SuccessMessage)
            {
                //on creation, text Host to confirm
                // string message = "Your party, " + Party_name + ", was successfully created!";
                // var returnedString = Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(Phone_number, message);

                //Todo: change to returnedString when texts implemented
                return StatusCode(200, new { message = result });
            }
            return StatusCode(500, new { message = result });
        }

        //Search for a particular party (Host) in database
        //GetHost query used for other endpoints - do we need this endpoint?
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

            return StatusCode(500, new { message = Common.Constants.Constants.FailedToGetHost });
        }

        //Search for a particular party (Host) in database
        //Used for the host login page (hence needing the password)
        [HttpGet("get-host-from-check-in-REMOVE-THIS")]
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

            var result = await _mediator.Send(new GetHostFromCheckIn.Query(){
                Party_code = party_code, 
                Phone_Number = phone_number, 
                Password = password
            });

            if(result != null)
            {
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = result });
        }
        #endregion

        #region Guest APIs

        //Adding a guest to a party from the host invitation flow
        //includes logic for open and closed invite parties 
        [HttpPost("add-guest-from-host")]
        public async Task<IActionResult> AddGuestFromHost(
            int host_invite_only,
            string guest_name,
            string party_code,
            string guest_username)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }
            //make sure that the user who is adding this guest to the invite list
            //is the user who is on the host object
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }
            var host = await _mediator.Send(new GetHost.Query{Party_code = party_code});
            var result = Common.Constants.Constants.HostNotValidatedMessage;

            if(host == null)
            {
                return StatusCode(500, new {message = Common.Constants.Constants.FailedToGetHost});
            }
            //if the user is the host of the party, let them add the guest
            else if(host.username == UN)
            {
                result = await _mediator.Send(new AddGuestFromHost.Command{ 
                    Guest_name = guest_name, 
                    Party_code = party_code, 
                    Username = guest_username,
                    Invite_only = host_invite_only 
                });

                if(result == Common.Constants.Constants.SuccessMessage)
                {
                    List<Guest> guest_list = await _mediator.Send(new GetAllGuests.Query() {Party_code = party_code});
                    return StatusCode(200, new { message = guest_list });
                }
            }
            else
            {
                return StatusCode(500, new {message = Common.Constants.Constants.MismatchedPartyToUserMessage});
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

            //get the party that we are trying to add the guest to
            Models.Host corresponding_host = await _mediator.Send(new GetHost.Query { Party_code = party_code });

            //validate the guest trying to join
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }
            var invited_guest = await _mediator.Send(new GetGuest.Query(){
                Guest_name = guest_name, 
                Party_code = party_code,
                Username = UN
            });

            //add the guest
            var result = await _mediator.Send(new AddGuestFromCheckIn.Command {
                Host = corresponding_host,
                Guest = invited_guest
            });

            //if the command says we are good to update the guest, update it
            if(result == Common.Constants.Constants.OkToUpdateGuestMessage)
            {
                result = await _mediator.Send(new UpdateGuest.Command{ 
                    Party_code = invited_guest.party_code,
                    At_party = 1,
                    Username = invited_guest.username
                });
            }
            //check if guest was let into the party (UpdateGuest returned success)
            if(result == Common.Constants.Constants.SuccessMessage)
            {
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = result });
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
            //make sure guest is validated with a username
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }
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

         //Get the list of current guests at a particular party
        [HttpGet("get-current-guests")]
        public async Task<IActionResult> GetCurrentGuests(string party_code)
        {   
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //validate the party that we are checking the guest list for exists
            Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
            if(host == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.HostDoesntExistForGuestMessage });
            }

            else
            {
                List<Guest> guest_list = await _mediator.Send(new GetCurrentGuests.Query() {Party_code = party_code});
                //if guest list is not empty, return the guest list
                if(guest_list != null)
                {
                    return StatusCode(200, new { message = guest_list });
                }
                //if it is empty, then there are no guests at that party
                else if(guest_list == null)
                {
                    return StatusCode(200, new { message = new List<Guest>(){} });
                }
            }
            //hopefully this is unreachable
            return StatusCode(500, new { message = Common.Constants.Constants.CouldntGetGuestListMessage });
        }

        //Get the list of all guests at a particular party
        [HttpGet("get-all-guests")]
        public async Task<IActionResult> GetAllGuests(string party_code)
        {   
            List<String> paramsList = new List<String>(){party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //validate the party that we are checking the guest list for exists
            Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
            if(host == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.HostDoesntExistForGuestMessage });
            }
            else
            {
                List<Guest> guest_list = await _mediator.Send(new GetAllGuests.Query() {Party_code = party_code});
                //if guest list is not empty, return the guest list
                if(guest_list != null)
                {
                    return StatusCode(200, new { message = guest_list });
                }
                //if it is empty, then there are no guests at that party
                else if(guest_list == null)
                {
                    return StatusCode(200, new { message = new List<Guest>(){} });
                }            
            }
            //hopefully this is unreachable
            return StatusCode(500, new { message = Common.Constants.Constants.CouldntGetGuestListMessage });
        }

        //Remove a guest permanently from the party (only host can do this)
        [HttpPost("delete-guest")]
        public async Task<IActionResult> DeleteGuest(string guest_username, string guest_name, string party_code)
        {
            List<String> paramsList = new List<String>(){guest_username, guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }
            
            //get username (should be the host)
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }
            //validate the party that we are deleting the guest for exists
            //and that the username matches
            Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
            if(host == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.HostDoesntExistForGuestMessage });
            }
            if(host.username != UN)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.MismatchedPartyToUserMessage});
            }

            var result = await _mediator.Send(new DeleteGuest.Command(){
                Party_code = party_code, 
                Guest_name = guest_name, 
                Username = guest_username
            });

            if(result == Common.Constants.Constants.SuccessMessage)
            {
                List<Guest> guest_list = await _mediator.Send(new GetAllGuests.Query() {Party_code = party_code});
                //if guest list is not empty, return the guest list
                if(guest_list != null)
                {
                    return StatusCode(200, new { message = guest_list });
                }
                else if(guest_list == null) //if it is empty, then there are no guests at that party
                {
                    return StatusCode(200, new { message = new List<Guest>(){} });
                }              
            }
            return StatusCode(500, new { message = "Failed to delete Guest from db" });
        }

        //Guest leaves a party but can re-join later
        //Won't show up on current guest list
        [HttpPost("leave-party")]
        public async Task<IActionResult> LeaveParty(string party_code, string guest_name)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //make sure the guest who is trying to leave is a validated user
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }
            var result = await _mediator.Send(new UpdateGuest.Command{ 
                Party_code = party_code,
                At_party = 0,
                Username = UN
            });

            //if successfully removed, return success
            if(result == Common.Constants.Constants.SuccessMessage)
            {
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = Common.Constants.Constants.FailedToLeavePartyMessage });
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

            return StatusCode(500, new { message = Common.Constants.Constants.FailedToGetPartyObjectsMessage });
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

            if(result == Common.Constants.Constants.SuccessMessage)
            {
                return StatusCode(200, new { message = result });
            }

            return StatusCode(500, new { message = Common.Constants.Constants.FailedToEndPartyMessage });
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

            //make sure that the user trying to add food is validated as host for this party
            Models.Host host = await _mediator.Send(new GetHost.Query() {Party_code = party_code});
            var UN = GetValidatedUsername();
            if(UN == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.UserNotValidatedMessage });
            }
            if(host.username == UN)
            {
                var result = await _mediator.Send(new AddFood.Command { Party_code = party_code, Item_name = item_name });

                if(result == Common.Constants.Constants.SuccessMessage)
                {
                    List<Food> food_list = await _mediator.Send(new GetCurrentFoods.Query() {Party_code = party_code});
                    if(food_list != null)
                    {
                        return StatusCode(200, new { message = food_list });
                    }
                    //if it is empty, then there are no guests at that party
                    else if(food_list == null)
                    {
                        return StatusCode(200, new { message = new List<Food>(){} });
                    }
                    return StatusCode(200, new { message = food_list });
                }
            }
            else if (host.username != UN)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.MismatchedPartyToUserMessage });
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

            //validate the party that we are checking the food list for exists
            Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
            if(host == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.HostDoesntExistForFoodMessage });
            }
            else
            {
                List<Food> food_list = await _mediator.Send(new GetCurrentFoods.Query() {Party_code = party_code});
                
                //if food list is not empty, return the food list
                if(food_list != null)
                {
                    return StatusCode(200, new { message = food_list });
                }
                //if it is empty, then there are no food items at that party
                else if(food_list == null)
                {
                    return StatusCode(200, new { message = new List<Food>(){} });
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

            //validate status parameter - these are the only 3 valid statuses
            if(status != "full" && status != "low" && status != "out")
            {
                return StatusCode(500, new { message = Common.Constants.Constants.InvalidFoodStatusMessage });
            }

            //validate the party that we are updating the food for exists
            Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
            if(host == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.HostDoesntExistForFoodMessage });
            }
            else
            {
                //change status of food item in database
                var result = await _mediator.Send(new UpdateFoodStatus.Command { Status = status, Item_name = item_name, Party_code = party_code });
                if(result == Common.Constants.Constants.SuccessMessage)
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
        }

        //Texts host when a guest reports food as low or out and updates status of item
        //Could re-use this endpoint and pass in "from host" value to determine whether to text host or guests
        //but that would be confusing to handle on front end
        [HttpPost("update-food-status-from-guest")]
        public async Task<IActionResult> UpdateFoodStatusFromGuest(
            string party_code, 
            string status, 
            string guest_name, 
            string item_name)
        {
            List<String> paramsList = new List<String>(){guest_name, party_code, status, item_name};
            if(Common.Validators.Validators.ValidateStringParameters(paramsList) == false)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.ParameterValidationMessage });
            }

            //valid status parameter - these are the only 3 valid statuses
            if(status != "full" && status != "low" && status != "out")
            {
                return StatusCode(500, new { message = Common.Constants.Constants.InvalidFoodStatusMessage });
            }

            //validate the party that we are updating the food for exists
            Models.Host host = await _mediator.Send(new GetHost.Query { Party_code = party_code });
            if(host == null)
            {
                return StatusCode(500, new { message = Common.Constants.Constants.HostDoesntExistForFoodMessage });
            }
            else
            {
                //change status of food item in database
                var result = await _mediator.Send(new UpdateFoodStatus.Command {
                    Status = status, 
                    Item_name = item_name, 
                    Party_code = party_code 
                });

                if(result == Common.Constants.Constants.SuccessMessage)
                {
                    //if successfully updated status, text the host to let them know
                    // string text = guest_name + " reported " + item_name + " as " + status;
                    // Common.TextMessagingHelpers.TextMessagingHelpers.SendSMSMessage(corresponding_host.phone_number, text);
                    return StatusCode(200, new { message = result });
                }
                return StatusCode(500, new { message = "Failed to report item status to host" });
            }
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

            if(result == Common.Constants.Constants.SuccessMessage)
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
