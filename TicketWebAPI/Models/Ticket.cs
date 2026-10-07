using Ardalis.SmartEnum;
using TicketWebAPI.Abstractions;

namespace TicketWebAPI.Models;

public sealed class Ticket :Entity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Guid UserId { get; set;  } 
    public User? User { get; set; }
    public TicketStatusEnum Status { get; set; } = TicketStatusEnum.Acik;
    public DateTimeOffset? ClosedAt { get; set; }
    public int? Rating { get; set; }
    public List<TicketReply> Replies { get; set; } = new();
}

public sealed class TicketStatusEnum : SmartEnum<TicketStatusEnum, string>
{
    public static readonly TicketStatusEnum Acik = new("Acik", "Açık");
    public static readonly TicketStatusEnum Cevaplandi = new("Cevaplandi", "Cevaplandı");
    public static readonly TicketStatusEnum Kapatildi = new("Kapatildi", "Kapatıldı");
    private TicketStatusEnum(string name, string value) : base(name, value)
    {
    
    }
}