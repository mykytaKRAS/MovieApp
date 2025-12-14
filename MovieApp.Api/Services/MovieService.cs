using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MovieApp.Api.DTOs;
using MovieApp.Api.Models;
using MovieApp.Api.Data;
using MovieApp.Api.Hubs;

namespace MovieApp.Api.Services
{
    public interface IMovieService
    {
        Task<IEnumerable<MovieDto>> GetAllMoviesAsync();
        Task<MovieDto?> GetMovieByIdAsync(int id);
        Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto, string? username = null);
        Task<MovieDto?> UpdateMovieAsync(int id, UpdateMovieDto updateMovieDto);
        Task<bool> DeleteMovieAsync(int id);
        Task<IEnumerable<MovieDto>> SearchMoviesAsync(string? title, string? genre, int? year);
    }

    public class MovieService : IMovieService
    {
        private readonly MovieDbContext _context;
        private readonly IHubContext<MovieHub> _hubContext;

        public MovieService(MovieDbContext context, IHubContext<MovieHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
        {
            var movies = await _context.Movies.ToListAsync();
            return movies.Select(MapToDto);
        }

        public async Task<MovieDto?> GetMovieByIdAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            return movie == null ? null : MapToDto(movie);
        }

        public async Task<MovieDto> CreateMovieAsync(CreateMovieDto createMovieDto, string? username = null)
        {
            var movie = new Movie
            {
                Title = createMovieDto.Title,
                Description = createMovieDto.Description,
                ReleaseYear = createMovieDto.ReleaseYear,
                Genre = createMovieDto.Genre,
                Rating = createMovieDto.Rating
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Broadcast to all connected clients via SignalR
            var activityMessage = $"{username ?? "Admin"} added \"{movie.Title}\" ({movie.ReleaseYear})";
            await _hubContext.Clients.All.SendAsync("ReceiveMovieActivity", new
            {
                message = activityMessage,
                movie = MapToDto(movie),
                timestamp = DateTime.UtcNow,
                action = "added"
            });

            return MapToDto(movie);
        }

        public async Task<MovieDto?> UpdateMovieAsync(int id, UpdateMovieDto updateMovieDto)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return null;
            }

            if (updateMovieDto.Title != null)
                movie.Title = updateMovieDto.Title;

            if (updateMovieDto.Description != null)
                movie.Description = updateMovieDto.Description;

            if (updateMovieDto.ReleaseYear.HasValue)
                movie.ReleaseYear = updateMovieDto.ReleaseYear.Value;

            if (updateMovieDto.Genre != null)
                movie.Genre = updateMovieDto.Genre;

            if (updateMovieDto.Rating.HasValue)
                movie.Rating = updateMovieDto.Rating.Value;

            await _context.SaveChangesAsync();

            return MapToDto(movie);
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return false;
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<MovieDto>> SearchMoviesAsync(string? title, string? genre, int? year)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.ToLower().Contains(title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(m => m.Genre != null && m.Genre.ToLower().Contains(genre.ToLower()));
            }

            if (year.HasValue)
            {
                query = query.Where(m => m.ReleaseYear == year.Value);
            }

            var movies = await query.ToListAsync();
            return movies.Select(MapToDto);
        }

        private static MovieDto MapToDto(Movie movie)
        {
            return new MovieDto
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