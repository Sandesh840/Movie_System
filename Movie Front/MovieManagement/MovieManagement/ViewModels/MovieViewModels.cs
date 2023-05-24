namespace MovieManagement.ViewModels
{
    public class MovieViewModels
    {
        public int? MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? UserId { get; set; }
        public int Genre { get; set; }
        public string? MovieLink { get; set; }
        public string? MoviePath { get; set; }
        public int Total { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public IFormFile? FormFile { get; set; }
    }
}
