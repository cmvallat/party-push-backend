using Models;

namespace DataLayer;

public interface IPartyService
{
    Task<bool> UpsertGuest(Guest guest);
    Task<bool> UpsertHost(Host host);
    Task<Guest> GetGuest(string party_code, string guest_name);
    Task<Host> GetHost(string party_code);
    Task<bool> DeleteGuest(Guest guest);
    Task<List<Guest>> GetGuestList(string party_code);
}