namespace TicketWebAPI.DTOS;

public sealed class TicketReplyDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = default!;
    public string  UserFullName { get; set; } = default!;
    public string UserRole { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
