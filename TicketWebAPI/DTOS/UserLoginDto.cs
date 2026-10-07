namespace TicketWebAPI.DTOS;

public sealed record UserLoginDto(
        string Email,
        string Password);

