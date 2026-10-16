using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using Route = RaceDay.API.Models.EventRoute;


namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<EventEnrolment> EventEnrolments { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Result> Results { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ---- Event -> Organiser (User) ----
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Category -> Event ----
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)          // 👈 use the collection
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix decimal precision warning
            modelBuilder.Entity<Category>()
                .Property(c => c.EntryFee)
                .HasPrecision(10, 2);

            // ---- EventRoute -> Event ----
            modelBuilder.Entity<EventRoute>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Routes)              // 👈 use the collection
                .HasForeignKey(r => r.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- EventEnrolment -> Event ----
            modelBuilder.Entity<EventEnrolment>()
                .HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- EventEnrolment -> Category ----
            modelBuilder.Entity<EventEnrolment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)          // 👈 FIX: point to Category.Enrolments
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- EventEnrolment -> Participant ----
            modelBuilder.Entity<EventEnrolment>()
                .HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)          // 👈 FIX: point to Participant.Enrolments
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Result -> EventEnrolment (one-to-one) ----
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<Result>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}