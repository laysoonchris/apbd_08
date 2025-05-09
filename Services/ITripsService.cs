using Tutorial8.Models;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
    Task<List<TripDTO>> GetTripsByClientId(int clientId);
    Task<int> CreateNewClient(Client client);
    Task<bool> RegisterClientToTrip(int clientId, int tripId);
    Task<bool> DeleteClientTrip(int clientId, int tripId);
}