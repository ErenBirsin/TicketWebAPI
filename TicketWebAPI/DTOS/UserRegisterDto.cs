namespace TicketWebAPI.DTOS;

public sealed record UserRegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password);
