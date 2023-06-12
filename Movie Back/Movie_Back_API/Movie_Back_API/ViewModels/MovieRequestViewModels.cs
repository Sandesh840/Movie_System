namespace Movie_Back_API.ViewModels
{
    public class MovieRequestViewModels
    {
        public string? SearchName { get; set; }
        public string? UserId { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}
