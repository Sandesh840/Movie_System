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
            if(id != 0)
            {
                string apiUrl = "https://localhost:7063/api/Movie";
                string queryString = $"?id={id}";

                HttpClient httpClient=new HttpClient();
                var response = await httpClient.GetAsync(apiUrl + queryString);
                var responseString=response.Content.ReadAsStringAsync();
                MovieViewModels? movie=new MovieViewModels();
                if (response.IsSuccessStatusCode)
                {
                    movie = JsonConvert.DeserializeObject<MovieViewModels>(responseString.Result);
                    if(movie.MovieReview != null)
                    {
                        foreach(var item in movie.MovieReview)
                        {
                            var user=await _userManager.FindByIdAsync(item.UserId);
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
            MovieReviewViewModels movieReviewViewModels=new MovieReviewViewModels();
            movieReviewViewModels.MovieId = movieId;
            movieReviewViewModels.Comments = comment;
            var user= await _userManager.GetUserAsync(User);
            if (user != null)
            {
                movieReviewViewModels.UserId=user.Id;
            }
            HttpClient httpClient=new HttpClient();
            //following line convert the viewmodel into json, class type into json type
            var jsonContent = new StringContent(JsonConvert.SerializeObject(movieReviewViewModels), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://localhost:7063/api/Movie/addReview", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                string responseBody=await response.Content.ReadAsStringAsync();
                return RedirectToAction("ViewMovieById?id=" + movieReviewViewModels.MovieId);
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return RedirectToAction("ViewMovieById?id=" + movieReviewViewModels.MovieId);
            }
        }
    }
}
