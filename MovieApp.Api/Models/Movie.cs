namespace MovieApp.Api.Models
{
    public class Movie
    {
        public int Id { get; set; }               // PK
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? Genre { get; set; }
        public double Rating { get; set; }       // 0..10
    }
}