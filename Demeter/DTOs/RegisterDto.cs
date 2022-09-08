using Demeter.Enums;

namespace Demeter.DTOs;

public class RegisterDto
{
    public string? Username { get; set; }    
    public string? DisplayName { get; set; }
    public string? Password { get; set; }
    public Role Role { get; set; }
}