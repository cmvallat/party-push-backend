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
        public async Task<IActionResult> UpsertHost([Required] string party_name, [Required] string party_code, [Required] string phone_number, [Required] string spotify_device_id, [Required] int invite_only )
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
            var insertTestHostValue = "INSERT INTO Host (party_name, party_code, phone_number, spotify_device_id, invite_only) VALUES (@party_name, @party_code, @phone_number, @spotify_device_id, @invite_only)";
            //var insertTestGuestValue = "INSERT INTO Guest (guest_name, party_code, at_party) VALUES ('Christian_Vallat', 'fuckyou', '1')";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(insertTestHostValue, conn);
            cmd.Parameters.AddWithValue("@party_name", party_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@phone_number", phone_number);
            cmd.Parameters.AddWithValue("@spotify_device_id", spotify_device_id);
            cmd.Parameters.AddWithValue("@invite_only", invite_only);

            // Execute the command and get the number of rows affected, then close the connection
            int rowsAffected = cmd.ExecuteNonQuery();
            conn.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return Ok(new { message = "A row was added to the db" });
            }
            
            //if nothing was added to the db, return error
            return StatusCode(500, new { message = "Failed to write to db" });
            
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
