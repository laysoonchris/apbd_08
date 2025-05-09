using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

//kontrollery z odpowiednimi statusami 
namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips); //Ok 200
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            var trips = await _tripsService.GetTripsByClientId(id);
            if (trips == null || !trips.Any())
                return NotFound(); //404 
            
            return Ok(trips); //Ok 200
        }
        
        [HttpPost("/api/clients")]
        public async Task<IActionResult> CreateNewClient([FromBody] Client client)
        {        
            if (!ModelState.IsValid ||
                     string.IsNullOrEmpty(client.FirstName) ||
                     string.IsNullOrEmpty(client.LastName) ||
                     string.IsNullOrEmpty(client.Email) ||
                     string.IsNullOrEmpty(client.Telephone) ||
                     string.IsNullOrEmpty(client.Pesel))
            {
                return BadRequest(); //błąd walidacji/ np. błędna implementacja/ niemożliwa rejestracja
            }

            var id = await _tripsService.CreateNewClient(client);
            return Created($"/api/clients/{id}", new { Id = id }); //Created 201
        }

        [HttpPut("/api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
        {
            var success = await _tripsService.RegisterClientToTrip(id, tripId);
            if (!success)
                return BadRequest(); //błąd walidacji/ np. błędna implementacja/ niemożliwa rejestracja
            
            return Ok("Client registration successful."); //Ok 200
        }

        [HttpDelete("/api/clients/{id}/trips/{tripId}")]
        public async Task<IActionResult> DeleteClientFromTrip(int id, int tripId)
        {
            var success = await _tripsService.DeleteClientTrip(id, tripId);
            if(!success)
                return NotFound(); //404
            
            return Ok("Client removed successfully."); //Ok 200
        }
    }
}
