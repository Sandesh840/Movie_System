using Movie_Back_API.Generic;
namespace Movie_Back_API.ViewModels
{
    public class MovieViewModels:GenericModel
    {
        public int? MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? UserId { get; set; }
        public int Genre { get; set; }
        public string? MovieLink { get; set; }
        public string? MoviePath { get; set; }
        public int? Total { get; set; }
    }
}
