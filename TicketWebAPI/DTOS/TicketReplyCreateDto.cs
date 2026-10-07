namespace TicketWebAPI.DTOS;

public sealed record TicketReplyCreateDto(
    Guid TicketId,
    Guid UserId,
    string Message);