namespace Tutorial8.Models.DTOs;

//DTO, które reprezentuje dane wycieczki
public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<CountryDTO> Countries { get; set; }
}

//DTO, reprezentujące nazwę kraju
public class CountryDTO
{
    public string Name { get; set; }
}