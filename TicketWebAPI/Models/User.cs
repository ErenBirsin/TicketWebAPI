using Ardalis.SmartEnum;
using TicketWebAPI.Abstractions;

namespace TicketWebAPI.Models;

public sealed class User : Entity
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRoleEnum Role { get; set; } = UserRoleEnum.Kullanici;
}

public sealed class UserRoleEnum : SmartEnum<UserRoleEnum,string>
{
    public static readonly UserRoleEnum Kullanici = new("Kullanıci", "Kullanıcı");
    public static readonly UserRoleEnum Yetkili = new("Yetkili", "Yetkili");

    private UserRoleEnum(string name,string value) : base(name, value)
    { 

    }
    
}
