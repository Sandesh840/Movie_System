using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieManagement.ViewModels;
using Newtonsoft.Json;
using System.Text;

namespace MovieManagement.Controllers
{
    public class MovieController : Controller
    {
        // public static int MovieIdCheck = 0;
        private readonly UserManager<IdentityUser> _userManager;
        public MovieController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        public IActionResult AddMovie()
        {
            return View();
        }
        public async Task<IActionResult> SaveMovie(MovieViewModels movieViewModels)
        {
            if (movieViewModels == null)
            {

                return RedirectToAction("AddMovie");
            }
            else
            {
                MovieMapViewModel movieMapViewModel = new MovieMapViewModel();
                movieMapViewModel.Title = movieViewModels.Title;
                movieMapViewModel.Description = movieMapViewModel.Description;
                movieMapViewModel.Genre = movieViewModels.Genre;
                movieMapViewModel.MovieLink = movieViewModels.MovieLink;
                if (movieViewModels.FormFile != null)
                {
                    if (movieViewModels.FormFile != null && movieViewModels.FormFile.Length > 0)
                    {
                        //get file name
                        var fileName = Path.GetFileName(movieViewModels.FormFile.FileName);
                        var combinePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads",
                            movieViewModels.FormFile.FileName);
                        using (var stream = new FileStream(combinePath, FileMode.Create))
                        {
                            movieViewModels.FormFile.CopyTo(stream);
                        }
                        //save the file to database
                        movieMapViewModel.MoviePath = "/Uploads/" + movieViewModels.FormFile.FileName;
                    }
                }
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    movieMapViewModel.UserId = user?.Id;
                }
                HttpClient httpClient = new HttpClient();
                var jsonContent = new StringContent(JsonConvert.SerializeObject(movieMapViewModel), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://localhost:7063/api/Movie/addmovie\r\n", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    //Request was successful
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return RedirectToAction("LandingPage", "LandingPage");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    return RedirectToAction("AddMovie");
                }
            }
        }


        [Authorize]
        public async Task<IActionResult> ViewMovieById(int id)
        {
            //if (MovieIdCheck != 0)
            //{
            //    id=MovieIdCheck;
            //}
            string? userId = null;
            var user1 = await _userManager.GetUserAsync(User);
            if (user1 != null)
            {
                userId = user1?.Id;
            }
            if (id != 0)
            {
                string apiUrl = "https://localhost:7063/api/Movie";
                string queryString = $"?id={id}&userId={userId}";

                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl + queryString);
                var responseString = response.Content.ReadAsStringAsync();
                MovieViewModels? movie = new MovieViewModels();
                if (response.IsSuccessStatusCode)
                {
                    movie = JsonConvert.DeserializeObject<MovieViewModels>(responseString.Result);
                    if (movie.MovieReview != null)
                    {
                        foreach (var item in movie.MovieReview)
                        {
                            var user = await _userManager.FindByIdAsync(item.UserId);
                            item.UserName = user.UserName;
                        }
                    }
                }
                return View(movie);

            }
            else
            {
                return RedirectToAction("LandingPage", "LandingPage");
            }
        }

        public async Task<IActionResult> AddReview(int movieId, string comment)
        {
            MovieReviewViewModels movieReviewViewModels = new MovieReviewViewModels();
            movieReviewViewModels.MovieId = movieId;
            movieReviewViewModels.Comments = comment;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                movieReviewViewModels.UserId = user.Id;
            }
            HttpClient httpClient = new HttpClient();
            //following line convert the viewmodel into json, class type into json type
            var jsonContent = new StringContent(JsonConvert.SerializeObject(movieReviewViewModels), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://localhost:7063/api/Movie/addReview", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // MovieIdCheck = movieReviewViewModels.MovieId;
                return RedirectToAction("ViewMovieById");
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return RedirectToAction("ViewMovieById?id=" + movieReviewViewModels.MovieId);
            }
        }
        public async Task<IActionResult> AddRating(int movieId, int rating)
        {
            MovieRatingViewModels movieRatingViewModels = new MovieRatingViewModels();
            movieRatingViewModels.MovieId = movieId;
            movieRatingViewModels.Rating = rating;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                movieRatingViewModels.UserId = user.Id;
            }
            HttpClient httpClient = new HttpClient();
            //following line convert the viewmodel into json, class type into json type
            var jsonContent = new StringContent(JsonConvert.SerializeObject(movieRatingViewModels), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://localhost:7063/api/Movie/addRating", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
               // MovieIdCheck = movieRatingViewModels.MovieId;
                return RedirectToAction("ViewMovieById");
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return RedirectToAction("ViewMovieById?id=" + movieRatingViewModels.MovieId);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUserFavourite(int MovieId, int isFavourite)
        {

            UserFavouriteViewModel userFavouriteViewModel = new UserFavouriteViewModel();
            userFavouriteViewModel.MovieId = MovieId;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                userFavouriteViewModel.UserId = user?.Id;
            }
            HttpClient httpClient = new HttpClient();
            if (isFavourite == 1)
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(userFavouriteViewModel), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://localhost:7063/api/movie/adduserfavourite", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    // Request was successful
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //MovieIdCheck = userFavouriteViewModel.MovieId;
                    return RedirectToAction("ViewMovieById");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    return RedirectToAction("ViewMovieById?id=" + userFavouriteViewModel.MovieId, "Movie");
                }
            }
            else
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(userFavouriteViewModel), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://localhost:7063/api/movie/removeuserfavourite", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    // Request was successful
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //MovieIdCheck = userFavouriteViewModel.MovieId;
                    return RedirectToAction("ViewMovieById");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    return RedirectToAction("ViewMovieById?id=" + userFavouriteViewModel.MovieId, "Movie");
                }
            }
        }
    }
}