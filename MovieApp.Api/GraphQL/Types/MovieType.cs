using MovieApp.Api.Models;

namespace MovieApp.Api.GraphQL.Types
{
    // Movie GraphQL type
    public class MovieType
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? Genre { get; set; }
        public double Rating { get; set; }

        public static MovieType FromModel(Movie movie)
        {
            return new MovieType
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                Genre = movie.Genre,
                Rating = movie.Rating
            };
        }
    }
}