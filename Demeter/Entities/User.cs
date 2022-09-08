using Demeter.Enums;

namespace Demeter.Entities;

public class User
{
    public int Id { get; set; }    
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public Role Role { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
}