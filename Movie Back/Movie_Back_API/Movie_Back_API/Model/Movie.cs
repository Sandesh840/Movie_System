using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Movie_Back_API.Generic;

namespace Movie_Back_API.Model
{
    public class Movie:GenericModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? UserId { get; set; }
        public MovieDetails? MovieDetails { get; set; }
        public List<MovieMedia>? MovieMedia { get; set; }
        public List<MovieRating>? MovieRating { get; set; }
        public List<MovieReview>? MovieReview { get; set; }
        public List<UserFavourite>? UserFavourite { get; set; }
        public List<UserMovieLog>? UserMovieLog { get; set; }


    }
}
