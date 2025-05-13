using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSystem.Models
{
    [Table("Venue")]
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }

        [Required]
        [MaxLength(100)]
        public string VenueName { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        public int Capacity { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Url]
        [MaxLength(300)]
        public string ImageURL { get; set; }
        
    }
}
