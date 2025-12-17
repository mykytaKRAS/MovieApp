using System.ComponentModel.DataAnnotations;

namespace MovieApp.Api.DTOs
{
    public class CreateMovieDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Release year is required")]
        [Range(1888, 2100, ErrorMessage = "Release year must be between 1888 and 2100")]
        public int ReleaseYear { get; set; }

        [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
        public string? Genre { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public double Rating { get; set; }
    }

    public class UpdateMovieDto
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Range(1888, 2100, ErrorMessage = "Release year must be between 1888 and 2100")]
        public int? ReleaseYear { get; set; }

        [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
        public string? Genre { get; set; }

        [Range(0, 10, ErrorMessage = "Rating must be between 0 and 10")]
        public double? Rating { get; set; }
    }

    public class MovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public string? Genre { get; set; }
        public double Rating { get; set; }
    }
}