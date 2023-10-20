using Models;

namespace DataLayer;

public interface IPartyService
{
    //Check-In API functions
    //Commands
    Task<string> AddGuestFromHost(string guest_name, string party_code);
    Task<string> AddGuestFromCheckIn(Guest guest);
    Task<string> UpdateGuest(Guest guest);
    Task<string> CreateParty(Host host);
    Task<string> DeleteGuest(string party_code, string guest_name);
    Task<string> EndParty(string party_code);

    //Queries
    Task<Guest> GetGuest(string party_code, string guest_name);
    Task<Host> GetHost(string party_code);
    Task<Host> GetHostFromCheckIn(string party_code, string phone_numner, string password);
    Task<List<Guest>> GetGuestList(string party_code);
    Task<List<Guest>> GetAllGuestList(string party_code);
    Task<User> GetUser(string username, string password, string phone_number);
    
    //Refreshment API functions
    //Commands
    Task<string> AddFoodItem(string party_code, string item_name);
    Task<string> RemoveFoodItem(string party_code, string item_name);
    Task<string> ChangeFoodStatus(string party_code, string status, string item_name);

    //Queries
    Task<List<Food>> GetCurrentFoodList(string party_code);

    //Todo: add spotify API functions when implementing feature
}