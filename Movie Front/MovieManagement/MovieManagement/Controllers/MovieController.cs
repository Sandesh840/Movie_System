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
    }
}
