using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Movie_Back_API.Generic;

namespace Movie_Back_API.Model
{
    public class MovieRating: GenericModel
    {
        public int Id { get; set; }
        public decimal? Rating { get; set; }
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
        public string? UserId { get; set; }

    }
}
