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

namespace Api.DemoController
{
    [ApiController]
    [Route("[controller]")]

    public class DemoController : ControllerBase
    {
        //right now the controller has logic in it - eventually move to command
        [HttpPost("upsert-host")]
        // Todo: change from async to sync
        public async Task<IActionResult> UpsertHost([Required][FromBody] HostObjectModel host)
        {
            //Eventually store in Secrets Manager when EC2 is up and running
            //then get the secret from the commented out function
            //string connString = await GetSecret();

            string party_name = host.party_name;
            string party_code = host.party_code;
            string phone_number = host.phone_number;
            string spotify_device_id = host.spotify_device_id;
            int invite_only = host.invite_only;

            // Set up connection string with server, database, user, and password
            string connString = "wouldntyouliketoknowweatherboy(thiswillbecorrectlocally)";

            // Create and open the connection to the db
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();

            // Create the SQL statements you want to execute
            var hostUpsertStatement = "INSERT INTO Host (party_name, party_code, phone_number, spotify_device_id, invite_only) VALUES (@party_name, @party_code, @phone_number, @spotify_device_id, @invite_only)";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostUpsertStatement, conn);
            cmd.Parameters.AddWithValue("@party_name", party_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@phone_number", phone_number);
            cmd.Parameters.AddWithValue("@spotify_device_id", spotify_device_id);
            cmd.Parameters.AddWithValue("@invite_only", invite_only);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            conn.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return Ok(new { message = "A Host was added to the db" });
            }
            
            //if nothing was added to the db, return error
            return StatusCode(500, new { message = "Failed to write to db" });
            
        }

        [HttpPost("upsert-guest")]
        // Todo: change from async to sync
        public async Task<IActionResult> UpsertGuest([Required][FromBody] GuestObjectModel guest)
        {
            //Eventually store in Secrets Manager when EC2 is up and running
            //then get the secret from the commented out function
            //string connString = await GetSecret();

            string guest_name = guest.guest_name;
            string party_code = guest.party_code;
            int at_party = guest.at_party;

            // Set up connection string with server, database, user, and password
            string connString = "wouldntyouliketoknowweatherboy(thiswillbecorrectlocally)";

            // Create and open the connection to the db
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();

            // Create the SQL statements you want to execute
            //remember!!! party_code is a foreign key, so the guest needs to be joining an existing party
            //meaning there needs to be an entry in Host with the same party_code
            var guestUpsertStatement = "INSERT INTO Guest (guest_name, party_code, at_party) VALUES (@guest_name, @party_code, @at_party)";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestUpsertStatement, conn);
            cmd.Parameters.AddWithValue("@guest_name", guest_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@at_party", at_party);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            conn.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return Ok(new { message = "A Guest was added to the db" });
            }
            
            //if nothing was added to the db, return error
            return StatusCode(500, new { message = "Failed to write to db" });
            
        }

        [HttpGet("get-host")]
        // Todo: change from async to sync
        public async Task<IActionResult> GetHostByPartyCode([Required] string party_code)
        {
            //Eventually store in Secrets Manager when EC2 is up and running
            //then get the secret from the commented out function
            //string connString = await GetSecret();

            // Set up connection string with server, database, user, and password
            string connString = "wouldntyouliketoknowweatherboy(thiswillbecorrectlocally)";

            // Create and open the connection to the db
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();

            // Create the SQL statements you want to execute
            // Todo: LIMIT-ing 1 right now; need to make party_code UNIQUE in db so there is no other option
            var hostSelectStatement = "SELECT * FROM Host WHERE party_code = @party_code LIMIT 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostSelectStatement, conn);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            HostObjectModel returnedObj = new HostObjectModel();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    //map response properties to returned object
                    returnedObj.party_name = reader.GetString("party_name");
                    returnedObj.party_code = reader.GetString("party_code");
                    returnedObj.phone_number = reader.GetString("phone_number");
                    returnedObj.spotify_device_id = reader.GetString("spotify_device_id");
                    returnedObj.invite_only = reader.GetInt32("invite_only");
                }
            }
            conn.Close();
            
            //if something was added to the db, return success
            if(returnedObj != null)
            {
                return Ok(returnedObj);
            }
            
            //if nothing was added to the db, return error
            return StatusCode(500, new { message = "Failed to get from db" });
            
        }

        [HttpGet("get-guest")]
        // Todo: change from async to sync
        public async Task<IActionResult> GetGuestByNameAndCode([Required] string guest_name, [Required] string party_code)
        {
            //Eventually store in Secrets Manager when EC2 is up and running
            //then get the secret from the commented out function
            //string connString = await GetSecret();

            // Set up connection string with server, database, user, and password
            string connString = "wouldntyouliketoknowweatherboy(thiswillbecorrectlocally)";
            
            // Create and open the connection to the db
            MySqlConnection conn = new MySqlConnection(connString);
            conn.Open();

            // Create the SQL statements you want to execute
            // Todo: LIMIT-ing 1 right now; need to make party_code and guest_name UNIQUE in db so there is no other option
            var guestSelectStatement = "SELECT * FROM Guest WHERE guest_name = @guest_name && party_code = @party_code LIMIT 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestSelectStatement, conn);
            cmd.Parameters.AddWithValue("@guest_name", guest_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            GuestObjectModel returnedObj = new GuestObjectModel();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    //map response properties to returned object
                    returnedObj.guest_name = reader.GetString("guest_name");
                    returnedObj.party_code = reader.GetString("party_code");
                    returnedObj.at_party = reader.GetInt32("at_party");
                }
            }
            conn.Close();
            
            //if something was added to the db, return success
            if(returnedObj != null)
            {
                return Ok(returnedObj);
            }
            
            //if nothing was added to the db, return error
            return StatusCode(500, new { message = "Failed to get from db" });
            
        }

        //this is the the format for the body of a Host endpoint
        public class HostObjectModel
        {
            public string party_name { get; set; }
            public string party_code { get; set; }
            public string phone_number { get; set; }
            public string spotify_device_id { get; set; }
            public int invite_only { get; set; }
        }

        //this is the the format for the body of a Guest endpoint
        public class GuestObjectModel
        {
            public string guest_name { get; set; }
            public string party_code { get; set; }
            public int at_party { get; set; }
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

        // eventually move into not accepting parameters, but just a single object?
        // public class HostObjectModel
        // {
        //     public string Property1 { get; set; }
        //     public string Property2 { get; set; }
        //     public string Property3 { get; set; }
        // }
        
    }
}
