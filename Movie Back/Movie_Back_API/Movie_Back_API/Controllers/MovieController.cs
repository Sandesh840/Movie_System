using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movie_Back_API.Data;
using Movie_Back_API.Model;
using Movie_Back_API.ViewModels;

namespace Movie_Back_API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public MovieController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        [HttpPost("addmovie")]
        public async Task<IActionResult> AddMovie([FromBody]MovieViewModels movieViewModels)
        {
            using var dbTran=_applicationDbContext.Database.BeginTransaction();
            try
            {
                Movie movie = new Movie();
                if (movieViewModels != null)
                {
                    movie.Title = movieViewModels.Title;
                    movie.Description = movieViewModels.Description;
                    movie.UserId = movieViewModels.UserId;
                    movie.CreatedDateTime=DateTime.Now;
                    _applicationDbContext.Movie.Add(movie);
                    _applicationDbContext.SaveChanges();
                    MovieDetails movieDetails=new MovieDetails();
                    movieDetails.MovieId = movie.Id;
                    movieDetails.Genra=movieViewModels.Genre;
                    movieDetails.MovieLink = movieViewModels.MovieLink;
                    movieDetails.CreatedDateTime=DateTime.Now;
                    _applicationDbContext.MovieDetails.Add(movieDetails);
                    _applicationDbContext.SaveChanges();
                    dbTran.Commit();
                    return Ok("Data Saved mSuccessfully");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                dbTran.Rollback();
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("allMovie")]
        public IActionResult GetAllMovie([FromQuery] MovieRequestViewModels movieRequest)
        {
            try
            {
                var allMovies=new List<Movie>();
                List<MovieViewModels> movieViewModels=new List<MovieViewModels>();
                if (movieRequest != null)
                {
                    if(movieRequest.SearchName != null && movieRequest.SearchName!="")
                    {
                        //search by name
                        if (movieRequest.Limit>0 && movieRequest.Offset >= 0)
                        {
                            allMovies= _applicationDbContext.Movie.Where(x => x.Title.Contains(movieRequest.SearchName))
                            .Include(x => x.MovieDetails)
                            .Include(x => x.MovieMedia).Skip(movieRequest.Offset).Take(movieRequest.Limit).ToList();
                        }
                        else
                        {
                            allMovies = _applicationDbContext.Movie.Where(x => x.Title.Contains(movieRequest.SearchName))
                                .Include(x => x.MovieDetails).Include(x => x.MovieMedia).ToList();
                        }
                    }
                    else
                    {
                        //without search
                        if(movieRequest.Limit>0 && movieRequest.Offset >= 0)
                        {
                            allMovies = _applicationDbContext.Movie
                                .Include(x => x.MovieDetails)
                                .Include(x => x.MovieMedia).Skip(movieRequest.Offset)
                                .Take(movieRequest.Limit).ToList();
                        }
                        else
                        {
                            allMovies = _applicationDbContext.Movie
                                .Include(x => x.MovieDetails)
                                .Include(x => x.MovieMedia).ToList();
                        }
                    }
                    if (allMovies != null)
                    {
                        foreach(var item in allMovies)
                        {
                            MovieViewModels movieViewModel1=new MovieViewModels();
                            movieViewModel1.Title = item.Title;
                            movieViewModel1.MovieId = item.Id;
                            movieViewModel1.CreatedDateTime = item.CreatedDateTime;
                            movieViewModel1.Description=item.Description;
                            movieViewModel1.Genre = item.MovieDetails.Genra;
                            movieViewModel1.MovieLink = item.MovieDetails.MovieLink;
                            movieViewModel1.MoviePath = item.MovieMedia?.FirstOrDefault()?.MediaPath;
                            movieViewModels.Add(movieViewModel1);
                        }
                        if(movieViewModels!= null)
                        {
                            movieViewModels[0].Total = _applicationDbContext.Movie.Count();
                        }  
                        return Ok(movieViewModels);
                    }
                }
                return NotFound();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
