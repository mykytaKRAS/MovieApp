using Microsoft.EntityFrameworkCore;
using MovieApp.Api.GraphQL.Types;
using MovieApp.Api.Data;

namespace MovieApp.Api.GraphQL.Queries
{
    public class MovieQuery
    {
        // Search movies by title (autocomplete/suggestions)
        public async Task<List<MovieType>> SearchMovies(
            string query,
            int limit,
            [Service] MovieDbContext context,
            [Service] ILogger<MovieQuery> logger)
        {
            logger.LogInformation("GraphQL: Searching movies with query: {Query}, limit: {Limit}", query, limit);

            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<MovieType>();
            }

            var movies = await context.Movies
                .Where(m => m.Title.ToLower().Contains(query.ToLower()))
                .OrderByDescending(m => m.Rating)
                .Take(limit)
                .ToListAsync();

            logger.LogInformation("GraphQL: Found {Count} movies", movies.Count);

            return movies.Select(MovieType.FromModel).ToList();
        }

        // Get single movie by ID
        public async Task<MovieType?> Movie(
            int id,
            [Service] MovieDbContext context,
            [Service] ILogger<MovieQuery> logger)
        {
            logger.LogInformation("GraphQL: Getting movie with ID: {Id}", id);

            var movie = await context.Movies.FindAsync(id);
            
            return movie == null ? null : MovieType.FromModel(movie);
        }

        // Get all movies with optional filters
        public async Task<List<MovieType>> Movies(
            string? genre,
            double? minRating,
            int? year,
            [Service] MovieDbContext context,
            [Service] ILogger<MovieQuery> logger)
        {
            logger.LogInformation("GraphQL: Getting movies - Genre: {Genre}, MinRating: {MinRating}, Year: {Year}", 
                genre, minRating, year);

            var query = context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(genre))
            {
                query = query.Where(m => m.Genre == genre);
            }

            if (minRating.HasValue)
            {
                query = query.Where(m => m.Rating >= minRating.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(m => m.ReleaseYear == year.Value);
            }

            var movies = await query
                .OrderByDescending(m => m.Rating)
                .ToListAsync();

            logger.LogInformation("GraphQL: Found {Count} movies", movies.Count);

            return movies.Select(MovieType.FromModel).ToList();
        }
    }
}