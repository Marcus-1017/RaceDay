using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<Enrolment> Enrolment { get; set; }
    public DbSet<Result> Result { get; set; }
    public DbSet<EventImage> EventImage { get; set; }
    public DbSet<ProfileImage> ProfileImage { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<User>().Property(u => u.FullName).HasMaxLength(100);
        modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(150);
        modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(100);
        modelBuilder.Entity<User>().Property(u => u.Role).HasMaxLength(20);

        modelBuilder.Entity<Event>().Property(e => e.EventName).HasMaxLength(100);
        modelBuilder.Entity<Event>().Property(e => e.Description).HasMaxLength(500);
        modelBuilder.Entity<Event>().Property(e => e.Location).HasMaxLength(150);
        modelBuilder.Entity<Event>().Property(e => e.EventType).HasMaxLength(20);
        modelBuilder.Entity<Event>().Property(e => e.EventDate).HasColumnType("date");
        modelBuilder.Entity<Event>().Property(e => e.Distance).HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Category>().Property(c => c.CategoryName).HasMaxLength(50);

        modelBuilder.Entity<Enrolment>().Property(e => e.EnrolmentStatus).HasMaxLength(20).HasDefaultValue("Pending");

        modelBuilder.Entity<Result>().Property(r => r.FinishTime).HasMaxLength(20);

        modelBuilder.Entity<EventImage>().Property(i => i.ImageURL).HasMaxLength(150);
        modelBuilder.Entity<ProfileImage>().Property(i => i.ImageURL).HasMaxLength(150);
    }
}
