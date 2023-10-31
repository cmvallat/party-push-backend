using Models;

namespace DataLayer;

public interface IPartyService
{
    //Host functions
    Task<string> AddHost(Host host);
    Task<Host> GetHost(string party_code);
    Task<Host> GetHostFromCheckIn(string party_code, string phone_numner, string password);
    Task<List<Host>> GetHostsFromUser(string username);

    //Guest functions
    Task<string> AddGuestFromHost(string guest_name, string party_code);
    Task<string> AddGuestFromCheckIn(Guest guest);
    Task<Guest> GetGuest(string party_code, string guest_name, string username);
    Task<List<Guest>> GetGuestsFromUser(string username);
    Task<List<Guest>> GetCurrentGuests(string party_code);
    Task<List<Guest>> GetAllGuests(string party_code);
    //used to change at_party status (at check-in time AND guest leaving party)
    Task<string> UpdateGuest(Guest guest);
    Task<string> DeleteGuest(string party_code, string guest_name);

    //both Host and Guest
    Task<string> EndParty(string party_code);
    
    //Food functions
    Task<string> AddFood(string party_code, string item_name);
    Task<List<Food>> GetCurrentFoods(string party_code);
    Task<string> UpdateFoodStatus(string party_code, string status, string item_name);
    Task<string> DeleteFood(string party_code, string item_name);

    //User functions
    Task<string> AddUser(string username, string password, string phone_number);
    Task<User> GetUser(string username, string password);

    //Todo: add spotify API functions when implementing feature
}