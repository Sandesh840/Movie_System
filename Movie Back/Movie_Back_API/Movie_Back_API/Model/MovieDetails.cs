using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Movie_Back_API.Generic;

namespace Movie_Back_API.Model
{
    public class MovieDetails:GenericModel
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie? Movie { get; set; }
        public int Genra { get; set; }
        public string? MovieLink { get; set; }
       
    }
}

