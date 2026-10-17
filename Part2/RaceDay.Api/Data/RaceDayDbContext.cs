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
}