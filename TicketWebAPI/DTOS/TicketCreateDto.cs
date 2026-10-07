namespace TicketWebAPI.DTOS;

public sealed record TicketCreateDto(
    Guid UserId,
    string Title,
    string Description);
