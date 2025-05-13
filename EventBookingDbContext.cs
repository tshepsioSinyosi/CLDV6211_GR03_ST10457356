using Microsoft.EntityFrameworkCore;

namespace EventSystem.Models
{
    public class EventBookingDbContext :DbContext
    {
        public EventBookingDbContext(DbContextOptions<EventBookingDbContext> options)
         : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Booking> Booking { get; set; }
    }
}
