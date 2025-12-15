using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using MovieApp.Api.Data;

namespace MovieApp.Api.GrpcServices
{
    public class MovieStatsService : MovieStats.MovieStatsBase
    {
        private readonly MovieDbContext _context;
        private readonly ILogger<MovieStatsService> _logger;

        public MovieStatsService(MovieDbContext context, ILogger<MovieStatsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Simple request-response: Get top movies
        public override async Task<TopMoviesResponse> GetTopMovies(
            TopMoviesRequest request, 
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Getting top {Limit} movies", request.Limit);

            var limit = request.Limit > 0 ? request.Limit : 10;

            var query = _context.Movies.AsQueryable();

            // Filter by genre if specified
            if (!string.IsNullOrEmpty(request.Genre))
            {
                query = query.Where(m => m.Genre == request.Genre);
            }

            var topMovies = await query
                .OrderByDescending(m => m.Rating)
                .Take(limit)
                .ToListAsync();

            var response = new TopMoviesResponse();
            
            foreach (var movie in topMovies)
            {
                response.Movies.Add(new MovieInfo
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Description = movie.Description ?? string.Empty,
                    ReleaseYear = movie.ReleaseYear,
                    Genre = movie.Genre ?? string.Empty,
                    Rating = movie.Rating
                });
            }

            _logger.LogInformation("gRPC: Returning {Count} movies", response.Movies.Count);
            return response;
        }
    }
}