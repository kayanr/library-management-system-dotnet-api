namespace LibraryManagement.Api.Models
{
    public class Book
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public required string Author { get; set; }

        public required string ISBN { get; set; }

        public int PublicationYear { get; set; }

        public bool Available { get; set; }
    }
}