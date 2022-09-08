using Demeter.Enums;

namespace Demeter.DTOs;

public class UserDto
{
    public string? DisplayName { get; set; }    
    public string? Username { get; set; }
    public string? Token { get; set; }

    public Role Role { get; set; }
}