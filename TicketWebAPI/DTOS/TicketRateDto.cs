namespace TicketWebAPI.DTOS;

public sealed record TicketRateDto(
    Guid TicketId,
    Guid UserId,
    int Rating);
