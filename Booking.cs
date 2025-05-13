using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSystem.Models
{
    [Table("Booking")]
    public class Booking
    {
        [Key]public int BookingID { get; set; }

        [Required]
        public int EventID { get; set; }
        public Event Event { get; set; }

        [MaxLength(100)]
        public string CustomerName { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string CustomerEmail { get; set; }


        public int NumberOfTickets { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        //public Event Event { get; set; }
    }
}
