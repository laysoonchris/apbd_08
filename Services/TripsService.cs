using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial8.Models;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    
    // Bierze wszystkie wycieczki oraz powiązane kraje
    public async Task<List<TripDTO>> GetTrips() //pobiera wszystkie dostępne dane wycieczki
    {
        var trips = new List<TripDTO>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var tripCmd = new SqlCommand("SELECT IdTrip, Name FROM Trip", conn); //wygląd zapytania sql
            using var reader = await tripCmd.ExecuteReaderAsync();//bierzemy tutaj informacje na temat wszystkich wycieczek
            while (await reader.ReadAsync())
            {
                trips.Add(new TripDTO
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Countries = new List<CountryDTO>()
                });
            }
            reader.Close();

            foreach (var trip in trips) //pobieramy informacje na temat kraju dla każdej wycieczki
            {
                var countryCmd = new SqlCommand(
                    "SELECT Country.Name FROM Country_Trip CT JOIN Country ON CT.IdCountry = Country.IdCountry WHERE CT.IdTrip = @IdTrip", conn); //wygląd zapytania sql: 
                countryCmd.Parameters.AddWithValue("@IdTrip", trip.Id);

                using var countryReader = await countryCmd.ExecuteReaderAsync();
                while (await countryReader.ReadAsync())
                {
                    trip.Countries.Add(new CountryDTO
                    {
                        Name = countryReader.GetString(0)
                    });
                }
                countryReader.Close();
            }
        }
        return trips;
    }
    
    public async Task<List<TripDTO>> GetTripsByClientId(int clientId) // pobiera informacje na temat wszystkich wycieczek danego klienta
    {
        var trips = new List<TripDTO>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand("SELECT T.IdTrip, T.Name FROM Client_Trip CT JOIN Trip T ON CT.IdTrip = T.IdTrip WHERE CT.IdClient = @IdClient", conn); //wygląd zapytania sql: pobiera informacje z 2 tablic i łaczy zeby dostep byl mozliwy
        cmd.Parameters.AddWithValue("@IdClient", clientId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            trips.Add(new TripDTO
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Countries = new List<CountryDTO>()
            });
        }
        reader.Close();

        foreach (var trip in trips) //pobieramy informacje na temat kraju dla każdej wycieczki
        {
            var countryCmd = new SqlCommand("SELECT Country.Name FROM Country_Trip CT JOIN Country ON CT.IdCountry = Country.IdCountry WHERE CT.IdTrip = @IdTrip", conn); //wygląd zapytania sql
            countryCmd.Parameters.AddWithValue("@IdTrip", trip.Id);

            using var countryReader = await countryCmd.ExecuteReaderAsync();
            while (await countryReader.ReadAsync())
            {
                trip.Countries.Add(new CountryDTO
                {
                    Name = countryReader.GetString(0)
                });
            }
            countryReader.Close();
        }

        return trips;
    }

    public async Task<int> CreateNewClient(Client client) //tworzy nam nowego klienta
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)", conn); //wygląd zapytania sql

        cmd.Parameters.AddWithValue("@FirstName", client.FirstName); //przypisujemy po kolei wartosci
        cmd.Parameters.AddWithValue("@LastName", client.LastName);
        cmd.Parameters.AddWithValue("@Email", client.Email);
        cmd.Parameters.AddWithValue("@Telephone", client.Telephone);
        cmd.Parameters.AddWithValue("@Pesel", client.Pesel); 

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result); //konwertujemy wynik
    }
    
    public async Task<bool> RegisterClientToTrip(int clientId, int tripId) //zapisuje klienta na konkretną wycieczke, jesli są dostępne miejsca
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var checkClient = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @Id", conn); //wygląd zapytania sql: wybiera konkretnego klienta
        checkClient.Parameters.AddWithValue("@Id", clientId);
        if (await checkClient.ExecuteScalarAsync() == null) return false;

        var checkTrip = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @TripId", conn); //wygląd zapytania sql: sprawdza maksymalną liczbe osob na danej wycieczce
        checkTrip.Parameters.AddWithValue("@TripId", tripId);
        var maxPeopleObj = await checkTrip.ExecuteScalarAsync();
        if (maxPeopleObj == null) return false;

        var currentCountCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @TripId", conn); //wygląd zapytania sql: zlicza liczbe wycieczek
        currentCountCmd.Parameters.AddWithValue("@TripId", tripId);
        int currentCount = (int)await currentCountCmd.ExecuteScalarAsync();

        if (currentCount >= (int)maxPeopleObj) return false;

        var registerCmd = new SqlCommand("INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) VALUES (@IdClient, @IdTrip, @Date)", conn); //wygląd zapytania sql: dodajemy trip do tabeli wycieczek
        registerCmd.Parameters.AddWithValue("@IdClient", clientId);
        registerCmd.Parameters.AddWithValue("@IdTrip", tripId);
        registerCmd.Parameters.AddWithValue("@Date", DateTime.Now);
        await registerCmd.ExecuteNonQueryAsync();

        return true;
    }
    
    public async Task<bool> DeleteClientTrip(int clientId, int tripId) //usuwa klienta z wycieczki
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var check = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", conn); //wygląd zapytania sql o klienta
        check.Parameters.AddWithValue("@IdClient", clientId);
        check.Parameters.AddWithValue("@IdTrip", tripId);
        if (await check.ExecuteScalarAsync() == null) return false;

        var delete = new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", conn); //wygląd zapytania sql: usuwamy dany trip danego klienta
        delete.Parameters.AddWithValue("@IdClient", clientId);
        delete.Parameters.AddWithValue("@IdTrip", tripId);
        await delete.ExecuteNonQueryAsync();

        return true;
    }
}