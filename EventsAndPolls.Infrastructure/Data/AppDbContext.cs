using EventsAndPolls.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsAndPolls.Infrastructure.Data;

public class AppDbContext : DbContext
{
     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

     public DbSet<Event> Events { get; set; }
     public DbSet<Poll> Polls { get; set; }
     public DbSet<PollOption> PollOptions { get; set; }
     public DbSet<Vote> Votes { get; set; }
     public DbSet<PollOptionGroup> PollOptionGroups { get; set; }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
     {
          base.OnModelCreating(modelBuilder);

          // Event configuration
          modelBuilder.Entity<Event>(entity =>
          {
               entity.HasKey(e => e.Id);
               entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
               entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
               entity.Property(e => e.OrganizerId).IsRequired();
          });

          // Poll configuration
          modelBuilder.Entity<Poll>(entity =>
          {
               entity.HasKey(p => p.Id);
               entity.Property(p => p.Question).IsRequired().HasMaxLength(500);

               entity.HasOne(p => p.Event)
                     .WithMany(e => e.Polls)
                     .HasForeignKey(p => p.EventId)
                     .OnDelete(DeleteBehavior.Cascade);
          });

          // PollOption configuration
          modelBuilder.Entity<PollOption>(entity =>
          {
               entity.HasKey(po => po.Id);
               entity.Property(po => po.Text).IsRequired().HasMaxLength(200);

               entity.HasOne(po => po.Poll)
                     .WithMany(p => p.Options)
                     .HasForeignKey(po => po.PollId)
                     .OnDelete(DeleteBehavior.Cascade);

               entity.HasOne(po => po.Group)
                     .WithMany(g => g.Options)
                     .HasForeignKey(po => po.GroupId)
                     .IsRequired(false)
                     .OnDelete(DeleteBehavior.NoAction);
          });

          // Vote configuration - FIX HERE
          modelBuilder.Entity<Vote>(entity =>
          {
               entity.HasKey(v => v.Id);

               entity.HasOne(v => v.Poll)
                     .WithMany(p => p.Votes)
                     .HasForeignKey(v => v.PollId)
                     .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade to NoAction

               entity.HasOne(v => v.PollOption)
                     .WithMany()
                     .HasForeignKey(v => v.PollOptionId)
                     .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade to NoAction
          });

          modelBuilder.Entity<PollOptionGroup>(entity =>
          {
               entity.HasKey(g => g.Id);
               entity.Property(g => g.Name).IsRequired().HasMaxLength(200);

               entity.HasOne(g => g.Poll)
                     .WithMany(p => p.OptionGroups)
                     .HasForeignKey(g => g.PollId)
                     .OnDelete(DeleteBehavior.Cascade);
          });

     }
}