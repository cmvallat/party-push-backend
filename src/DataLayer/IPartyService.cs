using Models;

namespace DataLayer;

public interface IPartyService
{
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

}