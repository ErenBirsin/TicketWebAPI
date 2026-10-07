namespace TicketWebAPI.DTOS;

public sealed class TicketDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string UserFullName { get; set; } = default!;
    public int ReplyCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
}
