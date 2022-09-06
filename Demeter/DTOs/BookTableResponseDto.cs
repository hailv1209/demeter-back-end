namespace Demeter.DTOs;

public class BookTableResponseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? Date { get; set; }
    public string? Time { get; set; }
    public int? NumPeople { get; set; }
    public string? Message { get; set; }
}