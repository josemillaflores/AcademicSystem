namespace StudentService.Domain.ValueObjects;

public record ContactInfo
{
    public string? Phone { get; }
    public string? Address { get; }
    public string? City { get; }
    public string? Country { get; }

    public ContactInfo()
    {
        Phone = null;
        Address = null;
        City = null;
        Country = null;
    }

    public ContactInfo(string? phone, string? address, string? city = null, string? country = null)
    {
        Phone = phone;
        Address = address;
        City = city;
        Country = country;
    }
}