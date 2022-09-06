namespace Demeter.DTOs;

public class PaginationResponseDto<T>
{
    public List<T>? Data { get; set; }
    public int Total { get; set; }
}