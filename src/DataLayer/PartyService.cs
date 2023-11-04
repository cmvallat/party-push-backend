using Dapper;
using Models;
using System.Data;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Common;

namespace DataLayer;

public class PartyService : IPartyService
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    
    public PartyService(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    #region Methods for Host APIs

     public async Task<string> AddHost(
            string username,
            string party_name, 
            string party_code, 
            string phone_number, 
            //string spotify_device_id, 
            int invite_only)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //Todo: eventually add spotify_device_id when implementing feature

                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("AddHost", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@un", username);
                cmd.Parameters.AddWithValue("@pn", party_name);
                cmd.Parameters.AddWithValue("@pc", party_code);
                cmd.Parameters.AddWithValue("@pnum", phone_number);
                //cmd.Parameters.AddWithValue("@spotify_device_id", spotify_device_id);
                cmd.Parameters.AddWithValue("@inv", invite_only);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                //if nothing was updated in the db, but not a SQL error, return generic error message
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on foreign key party_code
                if (ex.Number == 1062)
                {
                    return Common.Constants.Constants.DuplicateEntryMessage; 
                }

                // throw generic message for other SQL errors
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<Host> GetHost(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetHost", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@pc", party_code);

            // Execute the command and return the object, then close the connection
            Models.Host returnedObj = new Models.Host();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    //map response properties to returned object
                    returnedObj.username = reader.GetString("username");
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

    //Todo: delete after confirming with Nick that we don't need it
    public async Task<Host> GetHostFromCheckIn(string party_code, string phone_number, string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            // party_code is a primary key and thus unique constraint in db
            var hostSelectStatement = "SELECT * FROM Host WHERE party_code = @party_code AND phone_number = @phone_number AND username = @username";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostSelectStatement, connection);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@phone_number", phone_number);
            cmd.Parameters.AddWithValue("@username", username);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            Models.Host returnedObj = new Models.Host();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    //map response properties to returned object
                    returnedObj.username = reader.GetString("username");
                    returnedObj.party_name = reader.GetString("party_name");
                    returnedObj.party_code = reader.GetString("party_code");
                    returnedObj.phone_number = reader.GetString("phone_number");
                    returnedObj.spotify_device_id = reader.GetString("spotify_device_id");
                    returnedObj.invite_only = reader.GetInt32("invite_only");
                }
            }
            connection.Close();

            //de facto way to tell if our object is null - if primary keys are null
            if(returnedObj.party_code == null || returnedObj.username == null)
            {
                return null;
            }
            return returnedObj;
        }
    }

    public async Task<List<Host>> GetHostsFromUser(string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetHostsFromUser", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@un", username);

            // Execute the command and return the object, then close the connection
            List<Models.Host> returnedHostList = new List<Models.Host>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnedHostList.Add(new Host(){
                        username = reader.GetString("username"),
                        party_name = reader.GetString("party_name"),
                        party_code = reader.GetString("party_code"),
                        phone_number = reader.GetString("phone_number"),
                        spotify_device_id = reader.GetString("spotify_device_id"),
                        invite_only = reader.GetInt32("invite_only")
                    });
                }
            }
            connection.Close();

            return returnedHostList;
        }
    }
    #endregion

    #region Methods for Guest APIs
    public async Task<string> AddGuestFromHost(string guest_name, string party_code, string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("AddGuest", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@un", username);
                cmd.Parameters.AddWithValue("@gn", guest_name);
                cmd.Parameters.AddWithValue("@pc", party_code);
                cmd.Parameters.AddWithValue("@ap", 0);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                
                //if nothing was added to the db, but not a SQL error, return generic error message
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on unique constraint of guest_name and party_code
                if (ex.Number == 1062)
                {
                    return Common.Constants.Constants.GuestAlreadyInvitedOrAtPartyMessage;
                }

                // Handle other SQL errors if needed
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<string> AddGuestFromCheckIn(string username, string guest_name, string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("AddGuest", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@un", username);
                cmd.Parameters.AddWithValue("@gn", guest_name);
                cmd.Parameters.AddWithValue("@pc", party_code);
                cmd.Parameters.AddWithValue("@ap", 1);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                
                //if nothing was added to the db, but not a SQL error, return generic error message
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on unique constraint of guest_name and party_code
                if (ex.Number == 1062)
                {
                    return Common.Constants.Constants.GuestAlreadyAtPartyCheckInMessage; 
                }

                // Handle other SQL errors if needed
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<Guest> GetGuest(string party_code, string guest_name, string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetGuest", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@un", username);
            cmd.Parameters.AddWithValue("@gn", guest_name);
            cmd.Parameters.AddWithValue("@pc", party_code);

            // Execute the command and return the object, then close the connection
            Guest returnedObj = new Guest();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    //map response properties to returned object
                    returnedObj.username = reader.GetString("username");
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

    public async Task<List<Guest>> GetGuestsFromUser(string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetGuestsFromUser", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@un", username);

            // Execute the command and return the object, then close the connection
            List<Models.Guest> returnedGuestList = new List<Models.Guest>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnedGuestList.Add(new Guest(){
                        username = reader.GetString("username"),
                        guest_name = reader.GetString("guest_name"),
                        party_code = reader.GetString("party_code"),
                        at_party = reader.GetInt32("at_party")
                    });
                }
            }
            connection.Close();

            return returnedGuestList;
        }
    }

    public async Task<List<Guest>> GetCurrentGuests(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetCurrentGuests", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@pc", party_code);

            // Execute the command and return the object, then close the connection
            List<Guest> returnedObj = new List<Guest>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnedObj.Add(new Guest(){
                        username = reader.GetString("username"),
                        guest_name = reader.GetString("guest_name"),
                        party_code = reader.GetString("party_code"),
                        at_party = reader.GetInt32("at_party")
                    });
                }
            }
            connection.Close();

            //if the list of guests is not empty, return the list
            if(returnedObj.Count() != 0)
            {
                return returnedObj;
            }
            //if it is, return null
            else
            {
                return null;
            }
        }
    }

    public async Task<List<Guest>> GetAllGuests(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetAllGuests", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            List<Guest> returnedObj = new List<Guest>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnedObj.Add(new Guest(){
                        username = reader.GetString("username"),
                        guest_name = reader.GetString("guest_name"),
                        party_code = reader.GetString("party_code"),
                        at_party = reader.GetInt32("at_party")
                    });
                }
            }
            connection.Close();

            //if the list of guests is not empty
            if(returnedObj.Count() != 0)
            {
                return returnedObj;
            }
            //if it is, return null
            else
            {
                return null;
            }
        }
    }

    public async Task<string> UpdateGuest(string party_code, int at_party, string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("UpdateGuest", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@at", at_party);
                cmd.Parameters.AddWithValue("@un", username);
                cmd.Parameters.AddWithValue("@pc", party_code);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was updated in the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                
                //if nothing was updated in the db, but not a SQL error, return generic error message
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<string> DeleteGuest(string Party_code, string Guest_name, string Username)
    {
        //doesn't matter if they are at the party currently or not, we are deleting them forever
        //so we don't need at_party

        using(var connection = await _connectionFactory.GetConnection())
        {
            //MAKE SURE SQL_SAFE_UPDATES = 0 IN ORDER TO BE ABLE TO DELETE
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("DeleteGuest", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@gn", Guest_name);
            cmd.Parameters.AddWithValue("@pc", Party_code);
            cmd.Parameters.AddWithValue("@un", Username);

            // Execute the command and get the number of rows affected, then close the connection
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was deleted from the database, return success
            if(rowsAffected != 0)
            {
                return Common.Constants.Constants.SuccessMessage;
            }
            
            //if nothing was deleted from the db, return error
            return Common.Constants.Constants.GenericDatabaseErrorMessage;
        }
    }

    public async Task<string> EndParty(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            var hostRowsAffected = 0;
            //MAKE SURE SQL_SAFE_UPDATES = 0 IN ORDER TO BE ABLE TO DELETE
            //call the stored procedure with parameters
            MySqlCommand guest_cmd = new MySqlCommand("DeleteGuestFromEndParty", connection);
            MySqlCommand host_cmd = new MySqlCommand("DeleteHostFromEndParty", connection);
            MySqlCommand food_cmd = new MySqlCommand("DeleteFoodFromEndParty", connection);

            guest_cmd.CommandType = CommandType.StoredProcedure;
            host_cmd.CommandType = CommandType.StoredProcedure;
            food_cmd.CommandType = CommandType.StoredProcedure;

            guest_cmd.Parameters.AddWithValue("@pc", party_code);
            host_cmd.Parameters.AddWithValue("@pc", party_code);
            food_cmd.Parameters.AddWithValue("@pc", party_code);

            // Execute the command and get the number of rows affected, then close the connection
            try
            {
                var guestRowsAffected = guest_cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }

            try
            {
                var foodRowsAffected = food_cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
            
            try
            {
                hostRowsAffected = host_cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }

            connection.Close();
            
            //if the party/host was deleted, return success
            //if the host was deleted, we know the guests and food were deleted
            //because otherwise Host couldn't be deleted b/c it has foreign key party_code
            if(hostRowsAffected != 0)
            {
                return Common.Constants.Constants.SuccessMessage;
            }
            
            //if the party wasn't deleted from the database, return error
            return Common.Constants.Constants.GenericDatabaseErrorMessage;
        }
    }

    #endregion

    #region Methods for Food APIs
    public async Task<string> AddFood(string party_code, string item_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("AddFood", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@item", item_name);
                cmd.Parameters.AddWithValue("@pc", party_code);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                //if nothing was added to the db, return error
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                //Duplicate entry
                if (ex.Number == 1062)
                {
                    return Common.Constants.Constants.DuplicateEntryMessage; 
                }

                //throw generic message for other SQL errors
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<List<Food>> GetCurrentFoods(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetCurrentFoods", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@pc", party_code);

            // Execute the command and return the object, then close the connection
            List<Food> returnedObj = new List<Food>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnedObj.Add(new Food(){
                        item_name = reader.GetString("item_name"),
                        party_code = reader.GetString("party_code"),
                        status = reader.GetString("status")
                    });
                }
            }
            connection.Close();

            //if the list of guests is not empty, return the list
            if(returnedObj.Count() != 0)
            {
                return returnedObj;
            }
            //if it is, return null
            else
            {
                return null;
            }
        }
    }

    public async Task<string> UpdateFoodStatus(string party_code, string status, string item_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("UpdateFoodStatus", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pc", party_code);
                cmd.Parameters.AddWithValue("@item", item_name);
                cmd.Parameters.AddWithValue("@stat", status);

                // Execute the command and get the number of rows affected, then close the connection
                // Todo: wrap in try block and handle errors in catch
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was updated in the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                
                //if nothing was updated in the db, but not a SQL error, return generic error message
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }
    
    public async Task<string> DeleteFood(string party_code, string item_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //MAKE SURE SQL_SAFE_UPDATES = 0 IN ORDER TO BE ABLE TO DELETE
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("DeleteFood", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@item", item_name);
                cmd.Parameters.AddWithValue("@pc", party_code);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was deleted from the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                
                //if nothing was deleted from the db, return error
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                // Duplicate entry
                if (ex.Number == 1062)
                {
                    return Common.Constants.Constants.DuplicateEntryMessage; 
                }

                // throw generic message for other SQL errors
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<string> AddUser(string username, string password, string phone_number)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                //call the stored procedure with parameters
                MySqlCommand cmd = new MySqlCommand("AddUser", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@un", username);
                cmd.Parameters.AddWithValue("@pw", password);
                cmd.Parameters.AddWithValue("@pnum", phone_number);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return Common.Constants.Constants.SuccessMessage;
                }
                
                //if nothing was added to the db, but not a SQL error, return generic error message
                return Common.Constants.Constants.GenericDatabaseErrorMessage;
            }
            catch (MySqlException ex)
            {
                // Duplicate entry
                if (ex.Number == 1062)
                {
                    return Common.Constants.Constants.DuplicateUserMessage;
                }

                // Handle other SQL errors if needed
                return Common.Constants.Constants.GenericSqlExceptionMessage;
            }
        }
    }

    public async Task<User> GetUser(string username, string password)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            //Todo: include phone number in query??
            //call the stored procedure with parameters
            MySqlCommand cmd = new MySqlCommand("GetUser", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@un", username);
            cmd.Parameters.AddWithValue("@pw", password);

            // Execute the command and return the object, then close the connection
            List<User> returnedObj = new List<User>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    returnedObj.Add(new User(){
                        username = reader.GetString("username"),
                        password = reader.GetString("password"),
                        phone_number = reader.GetString("phone_number")
                    });
 
                }
            }
            connection.Close();

            //if the list of users is not empty, return the list
            if(returnedObj.Count() != 0)
            {
                return returnedObj.First();
            }
            //if it is, return null
            else
            {
                return null;
            }
        }
    }
    #endregion
}