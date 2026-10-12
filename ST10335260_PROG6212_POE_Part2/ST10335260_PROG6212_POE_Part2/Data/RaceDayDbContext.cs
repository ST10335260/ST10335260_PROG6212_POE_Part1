using Microsoft.EntityFrameworkCore;
using ST10335260_PROG6212_POE_Part2.Models;

namespace ST10335260_PROG6212_POE_Part2.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<WeatherInfo> WeatherInfos => Set<WeatherInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantId, e.EventCategoryId }).IsUnique();

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentId).IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role).WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser).WithMany()
            .HasForeignKey(e => e.OrganiserId);

        modelBuilder.Entity<EventCategory>()
            .HasOne(ec => ec.Event).WithMany()
            .HasForeignKey(ec => ec.EventId);
        modelBuilder.Entity<EventCategory>()
            .HasOne(ec => ec.Category).WithMany()
            .HasForeignKey(ec => ec.CategoryId);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Event).WithMany()
            .HasForeignKey(e => e.EventId);
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant).WithMany()
            .HasForeignKey(e => e.ParticipantId);
        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.EventCategory).WithMany()
            .HasForeignKey(e => e.EventCategoryId);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment).WithMany()
            .HasForeignKey(r => r.EnrolmentId);

        modelBuilder.Entity<WeatherInfo>()
            .HasOne(w => w.Event).WithMany()
            .HasForeignKey(w => w.EventId);
    }
}