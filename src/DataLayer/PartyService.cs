using Dapper;
using Models;
using System.Data;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;


namespace DataLayer;

public class PartyService : IPartyService
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    #region Service Setup
    
    public PartyService(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    #endregion

    #region DB calls and methods

    public async Task<Guest> GetGuest(string party_code, string guest_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            // Todo: LIMIT-ing 1 right now; need to make party_code and guest_name UNIQUE in db so there is no other option
            var guestSelectStatement = "SELECT * FROM Guest WHERE guest_name = @guest_name && party_code = @party_code LIMIT 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestSelectStatement, connection);
            cmd.Parameters.AddWithValue("@guest_name", guest_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            Guest returnedObj = new Guest();

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
            connection.Close();

            //de facto way to tell if our object is null
            if(returnedObj.party_code == null || returnedObj.guest_name == null)
            {
                return null;
            }
            return returnedObj;
        }
    }

     public async Task<Host> GetHost(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            // Todo: LIMIT-ing 1 right now; need to make party_code UNIQUE in db so there is no other option
            var hostSelectStatement = "SELECT * FROM Host WHERE party_code = @party_code LIMIT 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostSelectStatement, connection);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            Models.Host returnedObj = new Models.Host();

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
            connection.Close();

            //de facto way to tell if our object is null
            if(returnedObj.party_code == null)
            {
                return null;
            }
            return returnedObj;
        }
    }

    #endregion
}