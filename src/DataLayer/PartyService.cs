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
    
    public PartyService(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    #region Methods for Host APIs

     public async Task<string> AddHost(Host host)
    {
        string party_name = host.party_name;
        string party_code = host.party_code;
        string phone_number = host.phone_number;
        string spotify_device_id = host.spotify_device_id;
        int invite_only = host.invite_only;
        string password = host.password;

        using(var connection = await _connectionFactory.GetConnection())
        {
            try{
            // Create the SQL statements you want to execute
            var hostUpsertStatement = "INSERT INTO Host (party_name, party_code, phone_number, spotify_device_id, invite_only, password) VALUES (@party_name, @party_code, @phone_number, @spotify_device_id, @invite_only, @password)";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostUpsertStatement, connection);
            cmd.Parameters.AddWithValue("@party_name", party_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@phone_number", phone_number);
            cmd.Parameters.AddWithValue("@spotify_device_id", spotify_device_id);
            cmd.Parameters.AddWithValue("@invite_only", invite_only);
            cmd.Parameters.AddWithValue("@password", password);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return "Success!";
            }
            
                //if nothing was updated in the db, but not a SQL error, return generic error message
                return "Something went wrong creating a party";
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on foreign key party_code
                if (ex.Number == 1062)
                {
                    return "Error: duplicate party code. Could not create party"; 
                }

                // Handle other SQL errors if needed
                return "SQL Exception: Something went wrong creating a new party."; // rethrow the exception for unhandled errors
            }
        }
    }

    public async Task<Host> GetHost(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            // party_code is a primary key and thus unique constraint in db
            var hostSelectStatement = "SELECT * FROM Host WHERE party_code = @party_code";

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
                    returnedObj.password = reader.GetString("password");
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

    public async Task<Host> GetHostFromCheckIn(string party_code, string phone_number, string password)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            // party_code is a primary key and thus unique constraint in db
            var hostSelectStatement = "SELECT * FROM Host WHERE party_code = @party_code AND phone_number = @phone_number AND password = @password";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(hostSelectStatement, connection);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@phone_number", phone_number);
            cmd.Parameters.AddWithValue("@password", password);

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
                    returnedObj.password = reader.GetString("password");
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
    #endregion

    #region Methods for Guest APIs
    public async Task<string> AddGuestFromHost(string guest_name, string party_code)
    {
        string guestName = guest_name;
        string partyCode = party_code;

        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                // Create the SQL statements you want to execute
                //remember!!! party_code is a foreign key, so the guest needs to be joining an existing party
                //meaning there needs to be an entry in Host with the same party_code
                var guestInsertStatement = "INSERT INTO Guest (guest_name, party_code, at_party) VALUES (@guest_name, @party_code, 0)";

                //parameterize the statement with values from the API
                MySqlCommand cmd = new MySqlCommand(guestInsertStatement, connection);
                cmd.Parameters.AddWithValue("@guest_name", guest_name);
                cmd.Parameters.AddWithValue("@party_code", party_code);

                // Execute the command and get the number of rows affected, then close the connection
                // Todo: wrap in try block and handle errors in catch
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return "Success!";
                }
                
                //if nothing was added to the db, but not a SQL error, return generic error message
                return "Guest From Host was not added to the database.";
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on unique constraint of guest_name and party_code
                if (ex.Number == 1062)
                {
                    //Todo: distinguish between current and invited guest by checking at_party
                    //see bottom of file for starter code
                    return "You already have a guest invited to or currently at your party with this name. Please check your current and invited guest list or add a new guest.";
                }

                // Handle other SQL errors if needed
                return "SQL exception: Failed to add guest to the database with this party code.";
            }
        }
    }

    public async Task<string> AddGuestFromCheckIn(Guest guest)
    {
        string guest_name = guest.guest_name;
        string party_code = guest.party_code;
        int at_party = guest.at_party;

        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                // Create the SQL statements you want to execute
                //remember!!! party_code is a foreign key, so the guest needs to be joining an existing party
                //meaning there needs to be an entry in Host with the same party_code
                var guestInsertStatement = "INSERT INTO Guest (guest_name, party_code, at_party) VALUES (@guest_name, @party_code, @at_party)";

                //parameterize the statement with values from the API
                MySqlCommand cmd = new MySqlCommand(guestInsertStatement, connection);
                cmd.Parameters.AddWithValue("@guest_name", guest_name);
                cmd.Parameters.AddWithValue("@party_code", party_code);
                cmd.Parameters.AddWithValue("@at_party", at_party);

                // Execute the command and get the number of rows affected, then close the connection
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return "Success!";
                }
                
                //if nothing was added to the db, but not a SQL error, return generic error message
                return "Something went wrong adding guest from check-in";
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on unique constraint of guest_name and party_code
                if (ex.Number == 1062)
                {
                    //Todo: distinguish between current and invited guest by checking at_party
                    //see bottom of file for starter code
                    return "Someone with the same name is already invited to or joined this party. Please check that you spelled your name and the party code right, or try joining another party."; 
                }

                // Handle other SQL errors if needed
                return "Something went wrong with guest check-in, SQL error. We don't know."; // rethrow the exception for unhandled errors
            }
        }
    }

    public async Task<Guest> GetGuest(string party_code, string guest_name, string username)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            // Todo: LIMIT-ing 1 right now; need to make party_code and guest_name UNIQUE in db so there is no other option
            var guestSelectStatement = "SELECT * FROM Guest WHERE guest_name = @guest_name && party_code = @party_code && username = @username LIMIT 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(guestSelectStatement, connection);
            cmd.Parameters.AddWithValue("@guest_name", guest_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@username", username);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
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

    public async Task<List<Guest>> GetCurrentGuests(string party_code)
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

            //if the list of guests is not empty
            if(returnedObj.Count() != 0)
            {
                return returnedObj;
            }
            else if (returnedObj.Count() == 0) //if it is, return null
            {
                return null;
            }
            else //something else went wrong, throw exception
            {
                throw new Exception("Something went wrong getting the guest list from db");
            }
        }
    }

    public async Task<List<Guest>> GetAllGuests(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            var guestSelectStatement = "SELECT * FROM Guest WHERE party_code = @party_code;";

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

            //if the list of guests is not empty
            if(returnedObj.Count() != 0)
            {
                return returnedObj;
            }
            else if (returnedObj.Count() == 0) //if it is, return null
            {
                return null;
            }
            else //something else went wrong, throw exception
            {
                throw new Exception("Something went wrong getting the guest list from db");
            }
        }
    }

    public async Task<string> UpdateGuest(Guest guest)
    {
        string guest_name = guest.guest_name;
        string party_code = guest.party_code;
        int at_party = guest.at_party;

        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                // Create the SQL statements you want to execute
                //remember!!! party_code is a foreign key, so the guest needs to be joining an existing party
                //meaning there needs to be an entry in Host with the same party_code
                var guestUpdateStatement = "UPDATE Guest SET at_party = @at_party WHERE guest_name = @guest_name AND party_code = @party_code";

                //parameterize the statement with values from the API
                MySqlCommand cmd = new MySqlCommand(guestUpdateStatement, connection);
                cmd.Parameters.AddWithValue("@guest_name", guest_name);
                cmd.Parameters.AddWithValue("@party_code", party_code);
                cmd.Parameters.AddWithValue("@at_party", at_party);

                // Execute the command and get the number of rows affected, then close the connection
                // Todo: wrap in try block and handle errors in catch
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was updated in the db, return success
                if(rowsAffected != 0)
                {
                    return "Success!";
                }
                
                //if nothing was updated in the db, but not a SQL error, return generic error message
                return "Something wrong with updating the guest";
            }
            catch (MySqlException ex)
            {
                // throw SQL exception (message, not really an exception)
                return "SQL exception: Something went wrong with updating the guest at_party field.";
            }
        }
    }

    public async Task<string> DeleteGuest(string Party_code, string Guest_name)
    {
        //doesn't matter if they are at the party currently or not, we are deleting them forever
        //so we don't need at_party
        string party_code = Party_code;
        string guest_name = Guest_name;

        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            //remember, party_code is a foreign key, so the guest needs to be at an existing party
            //meaning there needs to be an entry in Host with the same party_code

            //MAKE SURE SQL_SAFE_UPDATES = 0 IN ORDER TO BE ABLE TO DELETE
            //To do this in MySQLWorkbench, run: SET SQL_SAFE_UPDATES = 0;
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
                return "Success!";
            }
            
            //if nothing was deleted from the db, return error
            return "Could not delete guest from database";
        }
    }

    public async Task<string> EndParty(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            int guestRowsAffected = 0;
            int hostRowsAffected = 0;
            // Create the SQL statements you want to execute

            //remember!!! party_code is a foreign key, so the guest needs to be at an existing party
            //meaning there needs to be an entry in Host with the same party_code

            //MAKE SURE SQL_SAFE_UPDATES = 0 IN ORDER TO BE ABLE TO DELETE
            //To do this in MySQLWorkbench, run: SET SQL_SAFE_UPDATES = 0;
            //Todo: add Food here
            var guestDeleteStatement = "DELETE FROM Guest WHERE party_code = @party_code;";
            var hostDeleteStatement = "DELETE FROM Host WHERE party_code = @party_code";

            //parameterize the statement with values from the API
            MySqlCommand guest_cmd = new MySqlCommand(guestDeleteStatement, connection);
            MySqlCommand host_cmd = new MySqlCommand(hostDeleteStatement, connection);
            guest_cmd.Parameters.AddWithValue("@party_code", party_code);
            host_cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and get the number of rows affected, then close the connection
            try
            {
                guestRowsAffected = guest_cmd.ExecuteNonQuery();
            }
            catch
            {
                return "Something went wrong with deleting the guests at this party";
            }
            //have to delete all the guests before the host
            //otherwise, the foreign key party_code used in Guest 
            //will create an error
            try
            {
                hostRowsAffected = host_cmd.ExecuteNonQuery();
            }
            catch
            {
                return "Something went wrong with deleting the Host object for this party";
            }

            connection.Close();
            
            //if the party/host was deleted, return success
            //if the host was deleted, we know the guests were deleted
            if(hostRowsAffected != 0)
            {
                return "Success!";
            }
            
            //if the party wasn't deleted from the db, return error
            return "Something went wrong deleting Host from database";
        }
    }

    #endregion

    #region Methods for Food APIs
    public async Task<string> AddFood(string party_code, string item_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            //remember, party_code is a foreign key, so the guest needs to be at an existing party
            //meaning there needs to be an entry in Host with the same party_code
            var status = "full";
            var foodAddStatement = "INSERT INTO Food (item_name, party_code, status) VALUES (@item_name, @party_code, @status)";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(foodAddStatement, connection);
            cmd.Parameters.AddWithValue("@item_name", item_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);
            cmd.Parameters.AddWithValue("@status", status);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was added to the db, return success
            if(rowsAffected != 0)
            {
                return "Success!";
            }
            
            //if nothing was added to the db, return error
            return "Could not add food item to the database";
        }
    }

    public async Task<List<Food>> GetCurrentFoods(string party_code)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            var foodSelectStatement = "SELECT * FROM Food WHERE party_code = @party_code";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(foodSelectStatement, connection);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            List<Food> returnedObj = new List<Food>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Food f = new Food();
                    f.item_name = reader.GetString("item_name");
                    f.party_code = reader.GetString("party_code");
                    f.status = reader.GetString("status");
                    returnedObj.Add(f);
                }
            }
            connection.Close();

            //if the list of guests is not empty
            if(returnedObj.Count() != 0)
            {
                return returnedObj;
            }
            else if (returnedObj.Count() == 0) //if it is, return null
            {
                return null;
            }
            else //something else went wrong, throw exception
            {
                throw new Exception("Something went wrong getting the guest list from db");
            }
        }
    }

    public async Task<string> UpdateFoodStatus(string party_code, string status, string item_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                // Create the SQL statements you want to execute
                //remember!!! party_code is a foreign key, so the guest needs to be joining an existing party
                //meaning there needs to be an entry in Host with the same party_code
                var foodUpdateStatement = "UPDATE Food SET status = @status WHERE party_code = @party_code AND item_name = @item_name";

                //parameterize the statement with values from the API
                MySqlCommand cmd = new MySqlCommand(foodUpdateStatement, connection);
                cmd.Parameters.AddWithValue("@item_name", item_name);
                cmd.Parameters.AddWithValue("@party_code", party_code);
                cmd.Parameters.AddWithValue("@status", status);

                // Execute the command and get the number of rows affected, then close the connection
                // Todo: wrap in try block and handle errors in catch
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was updated in the db, return success
                if(rowsAffected != 0)
                {
                    return "Success!";
                }
                
                //if nothing was updated in the db, but not a SQL error, return generic error message
                return "Something wrong with updating the food status";
            }
            catch (MySqlException ex)
            {
                // throw SQL exception (message, not really an exception)
                return "SQL exception: Something went wrong with updating the food 'status' field.";
            }
        }
    }
    
    public async Task<string> DeleteFood(string party_code, string item_name)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            //remember, party_code is a foreign key, so the guest needs to be at an existing party
            //meaning there needs to be an entry in Host with the same party_code

            //MAKE SURE SQL_SAFE_UPDATES = 0 IN ORDER TO BE ABLE TO DELETE
            //To do this in MySQLWorkbench, run: SET SQL_SAFE_UPDATES = 0;
            var foodDeleteStatement = "DELETE FROM Food WHERE item_name = @item_name AND party_code = @party_code";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(foodDeleteStatement, connection);
            cmd.Parameters.AddWithValue("@item_name", item_name);
            cmd.Parameters.AddWithValue("@party_code", party_code);

            // Execute the command and get the number of rows affected, then close the connection
            // Todo: wrap in try block and handle errors in catch
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();
            
            //if something was deleted from the db, return success
            if(rowsAffected != 0)
            {
                return "Success!";
            }
            
            //if nothing was deleted from the db, return error
            return "Could not delete food item from database";
        }
    }

    public async Task<string> AddUser(string username, string password, string phone_number)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            try
            {
                // Create the SQL statements you want to execute
                var userInsertStatement = 
                "INSERT INTO Users (username, password, phone_number) "
                + "VALUES (@username, @password, @phone_number)";

                //parameterize the statement with values from the API
                MySqlCommand cmd = new MySqlCommand(userInsertStatement, connection);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@phone_number", phone_number);

                // Execute the command and get the number of rows affected, then close the connection
                // Todo: wrap in try block and handle errors in catch
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                
                //if something was added to the db, return success
                if(rowsAffected != 0)
                {
                    return "Success!";
                }
                
                //if nothing was added to the db, but not a SQL error, return generic error message
                return "User was not added to the database.";
            }
            catch (MySqlException ex)
            {
                // Duplicate entry on unique constraint of guest_name and party_code
                if (ex.Number == 1062)
                {
                    return "You already have a user with this username. Please add a new user.";
                }

                // Handle other SQL errors if needed
                return "SQL exception: Failed to add user to the database with this username.";
            }
        }
    }

    public async Task<User> GetUser(string username, string password)
    {
        using(var connection = await _connectionFactory.GetConnection())
        {
            // Create the SQL statements you want to execute
            //Todo: include phone number in query??
            var userSelectStatement = 
            "SELECT * FROM Users WHERE username = @username " +
            "AND password = @password LIMIT 1;";

            //parameterize the statement with values from the API
            MySqlCommand cmd = new MySqlCommand(userSelectStatement, connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            // Execute the command and return the object, then close the connection
            // Todo: wrap in try block and handle errors in catch
            List<User> returnedObj = new List<User>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    User u = new User();
                    u.username = reader.GetString("username");
                    u.password = reader.GetString("password");
                    u.phone_number = reader.GetString("phone_number");
                    returnedObj.Add(u);
                }
            }
            connection.Close();

            //if the list of users is not empty
            if(returnedObj.Count() != 0)
            {
                return returnedObj.First();
            }
            else if (returnedObj.Count() == 0) //if it is, return null
            {
                return null;
            }
            else //something else went wrong, throw exception
            {
                throw new Exception("Something went wrong getting the guest list from db");
            }
        }
    }

    #endregion

    //Cutting board: when checking if duplicated entry is at_party, use this code:
    // var duplicated_guest = await _mediator.Send(new GuestQuery.Query() {Guest_name = guest_name, Party_code = party_code});
    // if(duplicated_guest.at_party = 1) //if at party
    // {
    //     throw new Exception("You already have a guest currently at your party with this name. Please check your current guest list or add a new guest."); 
    // }
    // else //it was 0, not at party
    // {
    //     throw new Exception("You already have an invited guest with this name. Please check your invited guest list or add a new guest."); 
    // }

}