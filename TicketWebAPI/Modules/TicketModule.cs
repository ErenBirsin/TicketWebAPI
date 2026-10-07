using Carter;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TicketWebAPI.Context;
using TicketWebAPI.DTOS;
using TicketWebAPI.Models;
using TS.Result;

namespace TicketWebAPI.Modules;

public sealed class TicketModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder group)
    {
        var app = group
            .MapGroup("tickets")
            .WithTags("Tickets")
            .RequireRateLimiting("fixed");

        app.MapPost(string.Empty, async (
            TicketCreateDto request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            bool isUserExists = await dbContext.Users.AnyAsync(p => p.Id == request.UserId, cancellationToken);
            if (!isUserExists)
            {
                var res = Result<string>.Failure(404, "Kullanıcı bulunamadı");
                return Results.NotFound(res);
            }

            var ticket = request.Adapt<Ticket>();
            dbContext.Tickets.Add(ticket);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(Result<string>.Succeed("Ticket başarıyla oluşturuldu"));
        }).Produces<Result<string>>();


        app.MapGet(string.Empty, async (
            Guid userId,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
            if (user is null)
            {
                var res = Result<string>.Failure(404, "Kullanıcı bulunamadı");
                return Results.NotFound(res);
            }

            var query = dbContext.Tickets.AsQueryable();

            if (user.Role == UserRoleEnum.Kullanici)
            {
                query = query.Where(p => p.UserId == userId);
            }

            var tickets = await query
            .Select(s => new TicketDto
            {
                Id = s.Id,
                Title = s.Title,
                Status = s.Status.Value,
                UserFullName = s.User!.FirstName + " " + s.User.LastName,
                ReplyCount = s.Replies.Count,
                CreatedAt = s.CreatedAt,
                ClosedAt = s.ClosedAt
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

            return Results.Ok(Result<List<TicketDto>>.Succeed(tickets));
        }).Produces<Result<List<TicketDto>>>();


        app.MapGet("/{id:guid}", async (
            Guid id,
            Guid userId,
            bool newestFirst,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
            if (user is null)
            {
                var res = Result<string>.Failure(404, "Kullanıcı bulunamadı");
                return Results.NotFound(res);
            }
            var ticket = await dbContext.Tickets
                .Where(p => p.Id == id)
                .Select(s => new TicketDetailDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    Status = s.Status.Value,
                    UserId = s.UserId,
                    UserFullName = s.User!.FirstName + " " + s.User.LastName,
                    Rating = s.Rating,
                    CreatedAt = s.CreatedAt,
                    ClosedAt = s.ClosedAt,
                    Replies = s.Replies
                        .OrderBy(r => r.CreatedAt)
                        .Select(r => new TicketReplyDto
                        {
                            Id = r.Id,
                            Message = r.Message,
                            UserFullName = r.User!.FirstName + " " + r.User.LastName,
                            UserRole = r.User.Role.Value,
                            CreatedAt = r.CreatedAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (ticket is null)
            {
                var res = Result<string>.Failure(404, "Ticket bulunamadı");
                return Results.NotFound(res);
            }

            if (user.Role == UserRoleEnum.Kullanici && ticket.UserId != userId)
            {
                var res = Result<string>.Failure(403, "Bu ticketı görüntüleme yetkiniz yok");
                return Results.Json(res, statusCode: 403);
            }

            if (newestFirst)
            {
                ticket.Replies.Reverse();
            }

            return Results.Ok(Result<TicketDetailDto>.Succeed(ticket));
        }).Produces<Result<TicketDetailDto>>();


        app.MapPost("/cevap-yaz", async (
            TicketReplyCreateDto request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);
            if (user is null)
            {
                var res = Result<string>.Failure(404, "Kullanıcı bulunamadı");
                return Results.NotFound(res);
            }
            var ticket = await dbContext.Tickets.FirstOrDefaultAsync(p => p.Id == request.TicketId, cancellationToken);
            if (ticket is null)
            {
                var res = Result<string>.Failure(404, "Ticket bulunamadı");
                return Results.NotFound(res);
            }

            if (ticket.Status == TicketStatusEnum.Kapatildi)
            {
                var res = Result<string>.Failure(400, "Kapatılmış bir ticket'a cevap yazılamaz");
                return Results.BadRequest(res);
            }
            if (user.Role == UserRoleEnum.Kullanici && ticket.UserId != user.Id)
            {
                var res = Result<string>.Failure(403, "Bu ticket'a cevap yazma yetkiniz yok");
                return Results.Json(res, statusCode: 403);
            }
            var reply = request.Adapt<TicketReply>();
            dbContext.TicketReplies.Add(reply);

            ticket.Status = user.Role == UserRoleEnum.Yetkili ? TicketStatusEnum.Cevaplandi : TicketStatusEnum.Acik;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(Result<string>.Succeed("Cevap başarıyla eklendi"));
        }).Produces<Result<string>>();


        app.MapPut("/kapat", async (
            TicketCloseDto request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                var res = Result<string>.Failure(404, "Kullanıcı bulunamadı");
                return Results.NotFound(res);
            }

            if (user.Role != UserRoleEnum.Yetkili)
            {
                var res = Result<string>.Failure(403, "Ticket'ı sadece yetkili kapatabilir");
                return Results.Json(res, statusCode: 403);
            }

            var ticket = await dbContext.Tickets.FirstOrDefaultAsync(p => p.Id == request.TicketId, cancellationToken);
            if (ticket is null)
            {
                var res = Result<string>.Failure(404, "Ticket bulunamadı");
                return Results.NotFound(res);
            }

            if (ticket.Status == TicketStatusEnum.Kapatildi)
            {
                var res = Result<string>.Failure(400, "Ticket zaten kapatılmış");
                return Results.BadRequest(res);
            }

            ticket.Status = TicketStatusEnum.Kapatildi;
            ticket.ClosedAt = DateTimeOffset.Now;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(Result<string>.Succeed("Ticket başarıyla kapatıldı"));
        }).Produces<Result<string>>();


        app.MapPut("/degerlendir", async (
            TicketRateDto request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                var res = Result<string>.Failure(400, "Puan 1 ile 5 arasında olmalıdır");
                return Results.BadRequest(res);
            }

            var ticket = await dbContext.Tickets.FirstOrDefaultAsync(p => p.Id == request.TicketId, cancellationToken);
            if (ticket is null)
            {
                var res = Result<string>.Failure(404, "Ticket bulunamadı");
                return Results.NotFound(res);
            }

            if (ticket.UserId != request.UserId)
            {
                var res = Result<string>.Failure(403, "Sadece ticket'ı açan kişi değerlendirme yapabilir");
                return Results.Json(res, statusCode: 403);
            }

            if (ticket.Status != TicketStatusEnum.Kapatildi)
            {
                var res = Result<string>.Failure(400, "Sadece kapatılmış ticket'lar değerlendirilebilir");
                return Results.BadRequest(res);
            }

            if (ticket.Rating is not null)
            {
                var res = Result<string>.Failure(400, "Bu ticket zaten değerlendirilmiş");
                return Results.BadRequest(res);
            }

            ticket.Rating = request.Rating;

            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(Result<string>.Succeed("Değerlendirmeniz için teşekkürler"));
        }).Produces<Result<string>>();


        app.MapDelete("/{id:guid}", async (
            Guid id,
            Guid userId,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
            if (user is null)
            {
                var res = Result<string>.Failure(404, "Kullanıcı bulunamadı");
                return Results.NotFound(res);
            }

            var ticket = await dbContext.Tickets.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (ticket is null)
            {
                var res = Result<string>.Failure(404, "Ticket bulunamadı");
                return Results.NotFound(res);
            }

            if (user.Role == UserRoleEnum.Kullanici)
            {
                if (ticket.UserId != userId)
                {
                    var res = Result<string>.Failure(403, "Bu ticket'ı silme yetkiniz yok");
                    return Results.Json(res, statusCode: 403);
                }

                bool hasReplies = await dbContext.TicketReplies.AnyAsync(p => p.TicketId == id, cancellationToken);
                if (hasReplies)
                {
                    var res = Result<string>.Failure(400, "Cevap yazılmış bir ticket silinemez");
                    return Results.BadRequest(res);
                }
            }

            ticket.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(Result<string>.Succeed("Ticket başarıyla silindi"));
        }).Produces<Result<string>>();
    }
}


