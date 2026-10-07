using Carter;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using TicketWebAPI;
using TicketWebAPI.Context;
using TicketWebAPI.Models;
using TicketWebAPI.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors();
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    string connection = builder.Configuration.GetConnectionString("SqlServer")!;
    opt.UseSqlServer(connection);
});
builder.Services.AddCarter();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>().AddProblemDetails();
builder.Services.AddResponseCompression(x => x.EnableForHttps = true);
builder.Services.AddRateLimiter(x =>
{
    x.AddFixedWindowLimiter("fixed", p =>
    {
        p.PermitLimit = 100;
        p.QueueLimit = 100;
        p.Window = TimeSpan.FromSeconds(1);
        p.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.Configure<AdminUserOptions>(builder.Configuration.GetSection("AdminUser"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminUserOptions>>().Value;

    bool isAdminExists = await dbContext.Users.AnyAsync(p => p.Role == UserRoleEnum.Yetkili);

    if (!isAdminExists)
    {
        User admin = new()
        {
            FirstName = adminOptions.FirstName,
            LastName = adminOptions.LastName,
            Email = adminOptions.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminOptions.Password),
            Role = UserRoleEnum.Yetkili
        };
        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
    }
}
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors(x => x
.AllowAnyOrigin()
.AllowAnyHeader()
.AllowAnyMethod()
.SetPreflightMaxAge(TimeSpan.FromMinutes(10))
);

app.UseResponseCompression();
app.UseRateLimiter();

app.MapCarter();

app.Run();

