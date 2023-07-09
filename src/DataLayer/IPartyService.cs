using Models;

namespace DataLayer;

public interface IPartyService
{
    //Check-In API functions
    Task<string> AddGuestFromHost(string guest_name, string party_code);
    Task<string> AddGuestFromCheckIn(Guest guest);
    Task<string> UpdateGuest(Guest guest);
    Task<string> CreateParty(Host host);
    Task<Guest> GetGuest(string party_code, string guest_name);
    Task<Host> GetHost(string party_code);
    Task<Host> GetHostFromCheckIn(string party_code, string phone_numner, string password);
    Task<string> DeleteGuest(string party_code, string guest_name);
    Task<List<Guest>> GetGuestList(string party_code);
    Task<string> EndParty(string party_code);
    
    //Refreshment API functions
    Task<List<Food>> GetCurrentFoodList(string party_code);
    Task<string> AddFoodItem(string party_code);
    Task<string> RemoveFoodItem(string party_code);
    Task<string> ReportFoodStatus(string party_code);
    Task<string> HostChangeFoodStatus(string party_code);

    //Todo: add spotify API functions when implementing feature
}