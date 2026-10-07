namespace TicketWebAPI.DTOS;

public sealed class TicketDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Status { get; set; } = default!;
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = default!;
    public int? Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public List<TicketReplyDto> Replies { get; set; } = new();

}
