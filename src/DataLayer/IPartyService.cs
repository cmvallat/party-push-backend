using Models;

namespace DataLayer;

public interface IPartyService
{
    //Task<bool> UpsertGuest(Guest guest);
    //Task<bool> UpsertHost(Host host);
    Task<Guest> GetGuest(string party_code, string guest_name);
    //Task<bool> GetHost(string party_code, string host_name);

}