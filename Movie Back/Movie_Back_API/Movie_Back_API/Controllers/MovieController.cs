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

                    MovieMedia movieMedia=new MovieMedia();
                    movieMedia.MovieId=movie.Id;
                    movieMedia.MediaPath=movieViewModels.MoviePath;
                    movieMedia.CreatedDateTime=DateTime.Now;
                    _applicationDbContext.MovieMedia.Add(movieMedia);
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

        [HttpGet()]
        public IActionResult GetMovieById([FromQuery] int id)
        {
            try
            {
                MovieViewModels movieViewModels=new MovieViewModels();
                var movie = _applicationDbContext.Movie.Where(x => x.Id == id).Include(x=>x.MovieDetails).Include(x=>x.MovieMedia).Include(x=>x.MovieReview).Include(x=>x.MovieRating).FirstOrDefault();
                if (movie != null)
                {
                    movieViewModels.MovieId=movie.Id;
                    movieViewModels.Title = movie.Title;
                    movieViewModels.CreatedDateTime=movie.CreatedDateTime;
                    movieViewModels.Description = movie.Description;
                    movieViewModels.Genre = movie.MovieDetails.Genra;
                    movieViewModels.MoviePath = movie.MovieMedia?.FirstOrDefault()?.MediaPath;
                    movieViewModels.MovieLink=movie.MovieDetails.MovieLink;
                    movieViewModels.MovieReview = new List<MovieReviewViewModels>();
                    if (movie.MovieReview != null)
                    {
                        foreach(var item in movie.MovieReview)
                        {
                            MovieReviewViewModels movieReviewViewModels=new MovieReviewViewModels();
                            movieReviewViewModels.MovieId=item.MovieId;
                            movieReviewViewModels.UserId=item.UserId;
                            movieReviewViewModels.Comments=item.Comments;
                            movieViewModels.MovieReview.Add(movieReviewViewModels);

                        }
                    }
                    movieViewModels.AvgRating = 0;
                    if (movie.MovieRating != null && movie.MovieRating.Count>0)
                    {
                        int rating = 0;
                        foreach(var item in movie.MovieRating)
                        {
                            rating=rating+Convert.ToInt32(item.Rating);
                        }
                        movieViewModels.AvgRating=rating/movie.MovieRating.Count;
                    }
                    return Ok(movieViewModels);
                }
                return NotFound();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMovie(int id)
        {
            try
            {
                var movie = _applicationDbContext.Movie.FirstOrDefault(x => x.Id == id);
                if (movie != null)
                {
                    _applicationDbContext.Movie.Remove(movie);
                    _applicationDbContext.SaveChanges();
                    return Ok("Movie deleted successfully.");
                }
                
                    return NotFound("Movie not found.");
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("addReview")]
        public IActionResult AddReview(MovieReviewViewModels movieReviewViewModels)
        {
            using var dbTran = _applicationDbContext.Database.BeginTransaction();
            try
            {
                MovieReview movieReview=new MovieReview();
                if (movieReview != null)
                {
                    movieReview.MovieId=movieReviewViewModels.MovieId;
                    movieReview.UserId=movieReviewViewModels.UserId;
                    movieReview.Comments = movieReviewViewModels.Comments;
                    _applicationDbContext.MovieReview.AddAsync(movieReview);
                    _applicationDbContext.SaveChanges();
                    dbTran.Commit();
                    return Ok(movieReview);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("addRating")]
        public IActionResult AddRating(MovieRatingViewModels movieRatingViewModels)
        {
            using var dbTran = _applicationDbContext.Database.BeginTransaction();
            try
            {
                MovieRating MovieRating = new MovieRating();
                if (MovieRating != null)
                {
                    MovieRating.MovieId = movieRatingViewModels.MovieId;
                    MovieRating.UserId = movieRatingViewModels.UserId;
                    MovieRating.Rating = movieRatingViewModels.Rating;
                    _applicationDbContext.MovieRating.Add(MovieRating);
                    _applicationDbContext.SaveChanges();
                    dbTran.Commit();
                    return Ok(MovieRating);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("updatemovie")]
        public async Task<IActionResult> UpdateMovie([FromBody] MovieViewModels movieViewModels)
        {
            using var dbTran = _applicationDbContext.Database.BeginTransaction();
            try
            {
                Movie movie = new Movie();
                if (movieViewModels != null)
                {
                    movie.Title = movieViewModels.Title;
                    movie.Description = movieViewModels.Description;
                    movie.UserId = movieViewModels.UserId;

                    _applicationDbContext.Movie.Update(movie);
                    _applicationDbContext.SaveChanges();

                    MovieDetails movieDetails = new MovieDetails();
                    movieDetails.MovieId = movie.Id;
                    movieDetails.Genra = movieViewModels.Genre;
                    movieDetails.MovieLink = movieViewModels.MovieLink;

                    _applicationDbContext.MovieDetails.Update(movieDetails);
                    _applicationDbContext.SaveChanges();

                    MovieMedia movieMedia = new MovieMedia();
                    movieMedia.MovieId = movie.Id;
                    movieMedia.MediaPath = movieViewModels.MoviePath;

                    _applicationDbContext.MovieMedia.Update(movieMedia);
                    _applicationDbContext.SaveChanges();

                    dbTran.Commit();
                    return Ok("Data Saved mSuccessfully");
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                dbTran.Rollback();
                return BadRequest(ex.Message);
            }

        }

    }
}
