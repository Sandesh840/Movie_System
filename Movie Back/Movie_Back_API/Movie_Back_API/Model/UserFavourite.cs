using Movie_Back_API.Generic;

namespace Movie_Back_API.Model
{
    public class UserFavourite : GenericModel
    {
        public int Id { get; set; }
        public int? MovieId { get; set; }
        public Movie? Movie { get; set; }
        public string? UserId { get; set; }
    }
}
