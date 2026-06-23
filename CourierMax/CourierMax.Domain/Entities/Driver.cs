namespace CourierMax.Domain.Entities;

public class Driver
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public Vehicle? Vehicle { get; private set; }

    private Driver()
    {
        Name = null!;
    }

    public Driver(string name, string? phone, string? email)
    {
        Name = name;
        Phone = phone;
        Email = email;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}
