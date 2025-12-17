using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using MovieApp.Api.Data;
using static MovieApp.Api.GrpcClients.MovieCalculator;

namespace MovieApp.Api.Endpoints
{
    public static class StatsEndpoints
    {
        public static void MapStatsEndpoints(this WebApplication app)
        {
            var statsGroup = app.MapGroup("/api/stats")
                .WithTags("Statistics via gRPC");

            // Calculate collection statistics using gRPC calculator
            statsGroup.MapGet("/collection", async (MovieDbContext dbContext) =>
            {
                try
                {
                    // Get all ratings from database
                    var ratings = await dbContext.Movies.Select(m => m.Rating).ToListAsync();

                    if (ratings.Count == 0)
                    {
                        return Results.Ok(new { message = "No movies in database" });
                    }

                    // Call gRPC Calculator Service
                    using var channel = GrpcChannel.ForAddress("http://localhost:5010");
                    var client = new MovieCalculatorClient(channel);

                    // Calculate average via gRPC
                    var ratingList = new MovieApp.Api.GrpcClients.RatingList();
                    ratingList.Ratings.AddRange(ratings);

                    var avgResponse = await client.CalculateAverageRatingAsync(ratingList);
                    var distResponse = await client.CalculateRatingDistributionAsync(ratingList);

                    var result = new
                    {
                        totalMovies = avgResponse.Count,
                        averageRating = avgResponse.Average,
                        highestRating = avgResponse.Highest,
                        lowestRating = avgResponse.Lowest,
                        distribution = new
                        {
                            excellent = distResponse.Excellent,
                            good = distResponse.Good,
                            average = distResponse.Average,
                            poor = distResponse.Poor
                        },
                        message = "Calculated via gRPC Calculator Service"
                    };

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"gRPC Calculator Service error: {ex.Message}");
                }
            })
            .WithName("GetCollectionStats")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK);

            // Get rating tier for a specific movie using gRPC
            statsGroup.MapGet("/tier/{movieId:int}", async (int movieId, MovieDbContext dbContext) =>
            {
                try
                {
                    var movie = await dbContext.Movies.FindAsync(movieId);
                    if (movie == null)
                    {
                        return Results.NotFound(new { message = "Movie not found" });
                    }

                    // Call gRPC Calculator Service
                    using var channel = GrpcChannel.ForAddress("http://localhost:5010");
                    var client = new MovieCalculatorClient(channel);

                    var request = new MovieApp.Api.GrpcClients.SingleRating { Rating = movie.Rating };
                    var response = await client.GetRatingTierAsync(request);

                    var result = new
                    {
                        movieId = movie.Id,
                        movieTitle = movie.Title,
                        rating = movie.Rating,
                        tier = response.Tier,
                        emoji = response.Emoji,
                        description = response.Description,
                        message = "Calculated via gRPC Calculator Service"
                    };

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"gRPC Calculator Service error: {ex.Message}");
                }
            })
            .WithName("GetMovieRatingTier")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK);
        }
    }
}