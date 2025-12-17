using Microsoft.AspNetCore.Mvc;
using MovieApp.Api.DTOs;
using MovieApp.Api.Services;

namespace MovieApp.Api.Endpoints
{
    public static class MovieEndpoints
    {
        public static void MapMovieEndpoints(this WebApplication app)
        {
            var moviesGroup = app.MapGroup("/api/movies")
                .WithTags("Movies");

            moviesGroup.MapGet("/", async (IMovieService movieService) =>
            {
                var movies = await movieService.GetAllMoviesAsync();
                return Results.Ok(movies);
            })
            .WithName("GetAllMovies")
            .Produces<IEnumerable<MovieDto>>(StatusCodes.Status200OK);

            moviesGroup.MapGet("/{id:int}", async (
                int id,
                IMovieService movieService) =>
            {
                var movie = await movieService.GetMovieByIdAsync(id);
                if (movie == null)
                {
                    return Results.NotFound(new { message = $"Movie with ID {id} not found" });
                }

                return Results.Ok(movie);
            })
            .WithName("GetMovieById")
            .Produces<MovieDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            moviesGroup.MapGet("/search", async (
                [FromQuery] string? title,
                [FromQuery] string? genre,
                [FromQuery] int? year,
                IMovieService movieService) =>
            {
                var movies = await movieService.SearchMoviesAsync(title, genre, year);
                return Results.Ok(movies);
            })
            .WithName("SearchMovies")
            .Produces<IEnumerable<MovieDto>>(StatusCodes.Status200OK);

            moviesGroup.MapPost("/", async (
                [FromBody] CreateMovieDto createMovieDto,
                IMovieService movieService,
                HttpContext context) =>
            {
                var username = context.User.Identity?.Name;
                var movie = await movieService.CreateMovieAsync(createMovieDto, username);
                return Results.Created($"/api/movies/{movie.Id}", movie);
            })
            .WithName("CreateMovie")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces<MovieDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

            moviesGroup.MapPut("/{id:int}", async (
                int id,
                [FromBody] UpdateMovieDto updateMovieDto,
                IMovieService movieService) =>
            {
                var movie = await movieService.UpdateMovieAsync(id, updateMovieDto);
                if (movie == null)
                {
                    return Results.NotFound(new { message = $"Movie with ID {id} not found" });
                }

                return Results.Ok(movie);
            })
            .WithName("UpdateMovie")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces<MovieDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

            moviesGroup.MapDelete("/{id:int}", async (
                int id,
                IMovieService movieService) =>
            {
                var result = await movieService.DeleteMovieAsync(id);
                if (!result)
                {
                    return Results.NotFound(new { message = $"Movie with ID {id} not found" });
                }

                return Results.NoContent();
            })
            .WithName("DeleteMovie")
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
        }
    }
}