namespace TicketWebAPI.DTOS;

public sealed record TicketCloseDto(
    Guid TicketId,
    Guid UserId);
