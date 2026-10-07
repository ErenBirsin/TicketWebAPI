using Microsoft.EntityFrameworkCore;
using TicketWebAPI.Models;

namespace TicketWebAPI.Context;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) :base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketReply> TicketReplies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(i => i.Role)
            .HasConversion(
                v => v.Value,
                v => UserRoleEnum.FromValue(v));
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);
            entity.Property(i=>i.Status)
            .HasConversion(
                v => v.Value,
                v => TicketStatusEnum.FromValue(v));

            entity
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

            entity
            .HasMany(p => p.Replies)
            .WithOne(p => p.Ticket)
            .HasForeignKey(p => p.TicketId)
            .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TicketReply>(entity =>
        {
            entity.HasQueryFilter(x => !x.IsDeleted);

            entity
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        });

    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach(var entry in ChangeTracker.Entries())
        {
            if(entry.Entity is Abstractions.Entity entity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTimeOffset.Now;
                        break;
                    case EntityState.Modified:
                        if (entity.IsDeleted)
                        {
                            entity.DeletedAt = DateTimeOffset.Now;
                        }
                        else
                        {
                            entity.UpdatedAt = DateTimeOffset.Now;
                        }
                        break;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

} 
