using System.ComponentModel.DataAnnotations;

namespace VideoStore.Models
{
    public class Video
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Genre { get; set; } = string.Empty;

        public int Year { get; set; }

        public decimal Price { get; set; }
    }
}
