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

        //Upsert Host endpoint
        [HttpPost("upsert-host")]

        // Todo: change from async to sync
        // Todo: add more validation for upserts??

        public async Task<IActionResult> UpsertHost([Required][FromBody] Models.Host host)
        {
            //validate input - make sure they passed a value for party_code, party_name and phone_number
            if(String.IsNullOrWhiteSpace(host.party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(host.party_name))
            {
                return StatusCode(500, new { message = "Party Name was invalid" });
            }

            if(String.IsNullOrWhiteSpace(host.phone_number))
            {
                return StatusCode(500, new { message = "Phone Number was invalid" });
            }

            var result = await _mediator.Send(new UpsertHost.Command { Host = host });

            if(result)
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to get upsert Host from db" });
        }

        //Upsert Guest endpoint
        [HttpPost("upsert-guest")]

        // Todo: change from async to sync
        public async Task<IActionResult> UpsertGuest([Required][FromBody] Guest guest)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(guest.party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(guest.guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }

           var result = await _mediator.Send(new UpsertGuest.Command { Guest = guest });

            if(result)
            {
                return Ok(result);
            }

            return StatusCode(500, new { message = "Failed to get upsert Guest from db" });
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
        public async Task<IActionResult> DeleteGuestByNameAndCode([Required][FromBody] Guest guest)
        {
            //validate input - make sure they passed a value for party_code and guest_name
            if(String.IsNullOrWhiteSpace(guest.party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            if(String.IsNullOrWhiteSpace(guest.guest_name))
            {
                return StatusCode(500, new { message = "Guest Name was invalid" });
            }

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
        public async Task<IActionResult> GetCurrentGuestsByCode([Required] string party_code)
        {   
            //validate input - make sure they passed a value for guest_name
            if(String.IsNullOrWhiteSpace(party_code))
            {
                return StatusCode(500, new { message = "Party Code was invalid" });
            }

            List<Guest> guest_list = await _mediator.Send(new CurrentGuestsQuery.Query() {Party_code = party_code});
            
            if(guest_list != null)
            {
                return Ok(guest_list);
            }

            return StatusCode(500, new { message = "Failed to get Guest list from db" });
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
