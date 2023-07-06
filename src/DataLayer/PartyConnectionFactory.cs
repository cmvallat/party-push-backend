using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataLayer
{
    public interface IDatabaseConnectionFactory
    {
        Task<MySqlConnection> GetConnection();
    }

    public class PartyConnectionFactory : IDatabaseConnectionFactory
    {
        //Todo: setup Secrets Manager here (TourLiveConnectionFactory)

        public async Task<MySqlConnection> GetConnection()
        {
            //Eventually store in Secrets Manager when EC2 is up and running
            //then get the secret from the commented out function
            //string connString = await GetSecret();

            // Set up connection string with server, database, user, and password
            //string connString = "wouldntyouliketoknowweatherboy(thiswillbecorrectlocally)";
            var conn = new MySqlConnection("server=party-resources.crurrv9mzw4i.us-west-1.rds.amazonaws.com;port=3306;database=Party;user=cmvallat;password=Gdtbath21");
            conn.Open();
            return conn;
        }
    }
    //Todo: use this code when adding Secrets Manager functionality
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