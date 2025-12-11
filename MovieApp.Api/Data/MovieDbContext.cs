using Microsoft.EntityFrameworkCore;
using MovieApp.Api.Models;

namespace MovieApp.Api.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }
        
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserToken> UserTokens { get; set; }

    }
}