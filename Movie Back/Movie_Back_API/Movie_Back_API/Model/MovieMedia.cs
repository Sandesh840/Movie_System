using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Movie_Back_API.Generic;

namespace Movie_Back_API.Model
{
    public class MovieMedia : GenericModel
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }
        public Movie? Movie { get; set; }
        public string? MediaPath { get; set; }
    }
}
