using Carter;
using Microsoft.EntityFrameworkCore;
using TicketWebAPI.Context;
using TicketWebAPI.DTOS;
using TicketWebAPI.Models;
using TS.Result;

namespace TicketWebAPI.Modules;

public sealed class UserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder group)
    {
        var app = group
            .MapGroup("users")
            .WithTags("Users")
            .RequireRateLimiting("fixed");

        app.MapPost("/register", async (
            UserRegisterDto request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            bool isMailExists = await dbContext.Users.AnyAsync(p => p.Email == request.Email, cancellationToken);
            if (isMailExists)
            {
                var res = Result<string>.Failure(400,"Bu eposta adresi ile kayıtlı bir kullanıcı zaten mevcut");
                return Results.BadRequest(res);
            }

            User user = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(Result<string>.Succeed("Kayıt işlemi başarıyla tamamlandı"));
        }).Produces<Result<string>>();

        app.MapPost("/login", async (
            UserLoginDto request,
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(p => p.Email == request.Email, cancellationToken);
            if(user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var res = Result<string>.Failure(400,"Eposta veya şifre hatalı");
                return Results.BadRequest(res);
            }

            var response = new
            {
                Id = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Role = user.Role.Value
            };

            return Results.Ok(Result<object>.Succeed(response));
        }).Produces<Result<object>>();

    }
}
