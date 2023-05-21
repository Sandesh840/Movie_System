using Microsoft.EntityFrameworkCore;
using Movie_Back_API.Model;

namespace Movie_Back_API.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<Movie> Movie { get; set; }
        public DbSet<MovieDetails> MovieDetails { get; set; }
        public DbSet<MovieMedia> MovieMedia { get; set; }
        public DbSet<MovieRating> MovieRating { get; set; }
        public DbSet<MovieReview> MovieReview { get; set; }
        public DbSet<UserFavourite> UserFavourite { get; set; }
        public DbSet<UserMovieLog> UserMovieLog { get; set; }       
    }
}
