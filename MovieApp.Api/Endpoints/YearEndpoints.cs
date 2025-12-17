using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using MovieApp.Api.Data;

namespace MovieApp.Api.Endpoints
{
    public static class YearEndpoints
    {
        public static void MapYearEndpoints(this WebApplication app)
        {
            // Get movie age - calculate locally (gRPC service optional)
            app.MapGet("/api/movies/{id:int}/age", async (int id, MovieDbContext dbContext) =>
            {
                var movie = await dbContext.Movies.FindAsync(id);
                if (movie == null)
                {
                    return Results.NotFound(new { message = "Movie not found" });
                }

                // Calculate years ago locally
                var currentYear = DateTime.Now.Year;
                var yearsAgo = currentYear - movie.ReleaseYear;
                
                var message = yearsAgo switch
                {
                    0 => "This year!",
                    1 => "Last year",
                    < 0 => $"{Math.Abs(yearsAgo)} years in the future",
                    _ => $"{yearsAgo} years ago"
                };

                // Try to call gRPC service (optional - falls back to local calculation)
                var calculatedVia = "Local calculation";
                try
                {
                    using var channel = GrpcChannel.ForAddress("http://localhost:5020");
                    var assembly = typeof(YearEndpoints).Assembly;
                    var clientType = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == "YearCalculatorClient");
                    
                    if (clientType != null)
                    {
                        var client = Activator.CreateInstance(clientType, channel);
                        calculatedVia = "gRPC Year Service (port 5020)";
                    }
                }
                catch
                {
                    // Silently fall back to local calculation
                }

                return Results.Ok(new
                {
                    id = movie.Id,
                    title = movie.Title,
                    releaseYear = movie.ReleaseYear,
                    yearsAgo = yearsAgo,
                    message = message,
                    calculatedVia = calculatedVia
                });
            })
            .WithTags("Movies")
            .WithName("GetMovieAge")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}