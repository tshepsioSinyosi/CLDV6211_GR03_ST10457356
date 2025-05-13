using System.ComponentModel.DataAnnotations;

namespace EventSystem.Models
{
    public class Event
    {
        [Key]public int EventID { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }
        public virtual Venue Venue { get; set; }


        [Required]
        public DateTime EventDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [Required]
        public int VenueID { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}
