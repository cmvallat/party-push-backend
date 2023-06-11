using Models;

namespace DataLayer;

public interface IPartyService
{
    Task<bool> AddGuestFromHost(Guest guest);
    Task<bool> AddGuestFromCheckIn(Guest guest);
    Task<bool> UpdateGuest(Guest guest);
    Task<bool> CreateParty(Host host);
    Task<Guest> GetGuest(string party_code, string guest_name);
    Task<Host> GetHost(string party_code);
    Task<bool> DeleteGuest(Guest guest);
    Task<List<Guest>> GetGuestList(string party_code);
    Task<bool> EndParty(string party_code);
}