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

            //de facto way to tell if our object is null - if primary keys are null
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

            //de facto way to tell if our object is null - if primary keys are null
            if(returnedObj.party_code == null)
            {
                return null;
            }
            return returnedObj;
        }
    }

    public async Task<bool> UpsertGuest(Guest guest)
    {
        string guest_name = guest.guest_name;
        string party_code = guest.party_code;
        int at_party = guest.at_party;

        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            //remember!!! party_code is a foreign key, so the guest needs to be joining an existing party
            //meaning there needs to be an entry in Host with the same party_code
            var guestUpsertStatement = "INSERT INTO Guest (guest_name, party_code, at_party) VALUES (@guest_name, @party_code, @at_party)";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestUpsertStatement, connection);
            cmd.Parameters.AddWithValue("@guest_name", guest_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@at_party", at_party);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return true;
            }
            
            //if nothing was added to the db, return error
            return false;
        }
    }

    public async Task<bool> UpsertHost(Host host)
    {
        string party_name = host.party_name;
        string party_code = host.party_code;
        string phone_number = host.phone_number;
        string spotify_device_id = host.spotify_device_id;
        int invite_only = host.invite_only;

        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            var hostUpsertStatement = "INSERT INTO Host (party_name, party_code, phone_number, spotify_device_id, invite_only) VALUES (@party_name, @party_code, @phone_number, @spotify_device_id, @invite_only)";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostUpsertStatement, connection);
            cmd.Parameters.AddWithValue("@party_name", party_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@phone_number", phone_number);
            cmd.Parameters.AddWithValue("@spotify_device_id", spotify_device_id);
            cmd.Parameters.AddWithValue("@invite_only", invite_only);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return true;
            }
            
            //if nothing was added to the db, return error
            return false;
        }
    }

    public async Task<bool> DeleteGuest(Guest guest)
    {
        string guest_name = guest.guest_name;
        string party_code = guest.party_code;

        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            //remember!!! party_code is a foreign key, so the guest needs to be at an existing party
            //meaning there needs to be an entry in Host with the same party_code

            //make sure SQL_SAFE_UPDATES = 1 in order to be able to delete
            //To do this in MySQLWorkbench, run: SET SQL_SAFE_UPDATES = 1;
            var guestDeleteStatement = "DELETE FROM Guest WHERE guest_name = @guest_name AND party_code = @party_code";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestDeleteStatement, connection);
            cmd.Parameters.AddWithValue("@guest_name", guest_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was deleted from the db, return success
            if(rowsAffected != 0)
            {
                return true;
            }
            
            //if nothing was deleted from the db, return error
            return false;
        }
    }
        
     public async Task<List<Guest>> GetGuestList(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            var guestSelectStatement = "SELECT * FROM Guest WHERE party_code = @party_code AND at_party = 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestSelectStatement, connection);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            List<Guest> returnedObj = new List<Guest>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Guest g = new Guest();
                    g.guest_name = reader.GetString("guest_name");
                    g.party_code = reader.GetString("party_code");
                    g.at_party = reader.GetInt32("at_party");
                    returnedObj.Add(g);
                }
            }
            connection.Close();

            //test if our list is null
            if(returnedObj != null)
            {
                return returnedObj;
            }
            return null;
        }
    }

    public async Task<bool> EndParty(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute

            //remember!!! party_code is a foreign key, so the guest needs to be at an existing party
            //meaning there needs to be an entry in Host with the same party_code

            //make sure SQL_SAFE_UPDATES = 1 in order to be able to delete
            //To do this in MySQLWorkbench, run: SET SQL_SAFE_UPDATES = 1;
            var guestDeleteStatement = "DELETE FROM Guest WHERE party_code = @party_code;";
            var hostDeleteStatement = "DELETE FROM Host WHERE party_code = @party_code";

            //parameterize the statement with values from the API
            MySqlCommand guest_cmd = new MySqlCommand(guestDeleteStatement, connection);
            MySqlCommand host_cmd = new MySqlCommand(hostDeleteStatement, connection);
            guest_cmd.Parameters.AddWithValue("@party_code", party_code);
            host_cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and get the number of rows affected, then close the connection
            try{
                int guestRowsAffected = guest_cmd.ExecuteNonQuery();
            }
            catch{
                throw new Exception("Something went wrong with deleting the guests at this party");
            }
            //have to delete all the guests before the host
            //otherwise, the foreign key party_code used in Guest 
            //will create an error
            int hostRowsAffected = host_cmd.ExecuteNonQuery();

            connection.Close();
            
            //if the party/host was deleted, return success
            //if the host was deleted, we know the guests were deleted
            if(hostRowsAffected != 0)
            {
                return true;
            }
            
            //if the party wasn't deleted from the db, return error
            return false;
        }
    }

    #endregion
}