using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models
{
    public class Book
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [Required]
        [StringLength(100)]
        public required string Author { get; set; }

        [Required]
        [StringLength(20)]
        public required string ISBN { get; set; }

        [Range(1, 9999)]
        public int PublicationYear { get; set; }

        public bool Available { get; set; }
    }
}