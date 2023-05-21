using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Movie_Back_API.Generic;

namespace Movie_Back_API.Model
{
    public class MovieReview : GenericModel
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }
        public string? Comments { get; set; }
        public string? UserId { get; set; }
        public Movie? Movie { get; set; }
    }
}
