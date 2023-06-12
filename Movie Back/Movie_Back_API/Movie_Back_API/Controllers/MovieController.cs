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

        /*
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
        }       */


        [HttpGet()]
        public IActionResult GetMovieById([FromQuery] int id, string? userId)
        {
            try
            {
                MovieViewModels movieViewModel = new MovieViewModels();
                var movie = _applicationDbContext.Movie.Where(x => x.Id == id)
                           .Include(x => x.MovieDetails)
                           .Include(x => x.MovieMedia).Include(x => x.MovieReview)
                           .Include(x => x.MovieRating).FirstOrDefault();
                if (movie != null)
                {
                    movieViewModel.MovieId = movie.Id;
                    movieViewModel.Title = movie.Title;
                    movieViewModel.CreatedDateTime = movie.CreatedDateTime;
                    movieViewModel.Description = movie.Description;
                    movieViewModel.Genre = movie.MovieDetails.Genra;
                    movieViewModel.MovieLink = movie.MovieDetails.MovieLink;
                    movieViewModel.MoviePath = movie.MovieMedia?.FirstOrDefault()?.MediaPath;
                    movieViewModel.MovieReview = new List<MovieReviewViewModels>();
                    if (movie.MovieReview != null)
                    {
                        foreach (var item in movie.MovieReview)
                        {
                            MovieReviewViewModels movieReviewViewModel = new MovieReviewViewModels();
                            movieReviewViewModel.MovieId = item.MovieId;
                            movieReviewViewModel.UserId = item.UserId;
                            movieReviewViewModel.Comments = item.Comments;
                            movieViewModel.MovieReview.Add(movieReviewViewModel);
                        }
                    }
                    movieViewModel.IsFavourite = _applicationDbContext.UserFavourite.
                        Where(x => x.MovieId == id && x.UserId == userId).FirstOrDefault() != null ?
                         1 : 0;
                    movieViewModel.AvgRating = 0;
                    if (movie.MovieRating != null && movie.MovieRating.Count > 0)
                    {
                        int rating = 0;
                        foreach (var item in movie.MovieRating)
                        {
                            rating = rating + Convert.ToInt32(item.Rating);
                        }
                        movieViewModel.AvgRating = rating / movie.MovieRating.Count;
                    }
                    return Ok(movieViewModel);

                }
                return NotFound();
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


        [HttpPost("adduserfavourite")]
        public IActionResult AddUserFavourite(UserFavouriteViewModel userFavouriteViewModel)
        {
            using var dbTran = _applicationDbContext.Database.BeginTransaction();
            try
            {
                //create a object of movie to map on movieViewModel
                UserFavourite userFavourite = new UserFavourite();
                if (userFavouriteViewModel.UserId != null && userFavouriteViewModel.MovieId != 0)
                {
                    userFavourite.MovieId = userFavouriteViewModel.MovieId;
                    userFavourite.UserId = userFavouriteViewModel.UserId;
                    userFavourite.CreatedDateTime = DateTime.Now;
                    _applicationDbContext.UserFavourite.Add(userFavourite);
                    _applicationDbContext.SaveChanges();
                    dbTran.Commit();
                    return Ok(userFavourite);

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

        [HttpPost("removeuserfavourite")]
        public IActionResult RemoveUserFavourite(UserFavouriteViewModel userFavouriteViewModel)
        {
            using var dbTran = _applicationDbContext.Database.BeginTransaction();
            try
            {
                //create a object of movie to map on movieViewModel
                UserFavourite? userFavourite = _applicationDbContext.UserFavourite.Where(x => x.UserId == userFavouriteViewModel.UserId
                 && x.MovieId == userFavouriteViewModel.MovieId).FirstOrDefault();
                if (userFavourite != null)
                {

                    _applicationDbContext.UserFavourite.Remove(userFavourite);
                    _applicationDbContext.SaveChanges();
                    dbTran.Commit();
                    return Ok(userFavourite);

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

        [HttpGet("getuserfavourite")]
        public IActionResult GetUserFavourite([FromQuery] MovieRequestViewModels movieRequest)
        {
            try
            {
                var allMovies = new List<Movie>();
                List<MovieViewModels> movieViewModels = new List<MovieViewModels>();
                List<int> favouriteMovieIdList = _applicationDbContext.UserFavourite.Where(x => x.UserId == movieRequest.UserId).
                    Select(x => x.MovieId).ToList();
                if (favouriteMovieIdList != null)
                {
                    if (movieRequest != null)
                    {
                        if (movieRequest.SearchName != null
                            && movieRequest.SearchName != "")
                        {
                            //search movie by name
                            //offset is used to skip the data while Limit value
                            //is used to get specific count of data
                            if (movieRequest.Limit > 0 && movieRequest.Offset >= 0)
                            {
                                allMovies = _applicationDbContext.Movie.Where(x => x.Title.Contains(movieRequest.SearchName) &&
                                 favouriteMovieIdList.Any(y => x.Id == y))
                                .Include(x => x.MovieDetails)
                                .Include(x => x.MovieMedia).Skip(movieRequest.Offset)
                                .Take(movieRequest.Limit).ToList();
                            }
                            else
                            {
                                allMovies = _applicationDbContext.Movie.Where(x => x.Title.Contains(movieRequest.SearchName) &&
                                favouriteMovieIdList.Any(y => x.Id == y))
                                .Include(x => x.MovieDetails)
                                .Include(x => x.MovieMedia).ToList();
                            }
                        }
                        else
                        {
                            //without search
                            if (movieRequest.Limit > 0 && movieRequest.Offset >= 0)
                            {
                                allMovies = _applicationDbContext.Movie.Where(x => favouriteMovieIdList.Any(y => x.Id == y))
                                .Include(x => x.MovieDetails)
                                .Include(x => x.MovieMedia).Skip(movieRequest.Offset)
                                .Take(movieRequest.Limit).ToList();
                            }
                            else
                            {
                                allMovies = _applicationDbContext.Movie.Where(x => favouriteMovieIdList.Any(y => x.Id == y))
                                .Include(x => x.MovieDetails)
                                .Include(x => x.MovieMedia).ToList();
                            }
                        }
                        if (allMovies != null)
                        {
                            foreach (var item in allMovies)
                            {
                                MovieViewModels movieViewModel = new MovieViewModels();
                                movieViewModel.MovieId = item.Id;
                                movieViewModel.Title = item.Title;
                                movieViewModel.CreatedDateTime = item.CreatedDateTime;
                                movieViewModel.Description = item.Description;
                                movieViewModel.Genre = item.MovieDetails.Genra;
                                movieViewModel.MovieLink = item.MovieDetails.MovieLink;
                                movieViewModel.MoviePath = item.MovieMedia?.FirstOrDefault()?.MediaPath;
                                movieViewModels.Add(movieViewModel);
                            }
                            if (movieViewModels != null)
                            {
                                movieViewModels[0].Total = _applicationDbContext.Movie.Where(x => favouriteMovieIdList.Any(y => x.Id == y)).Count();
                            }
                            return Ok(movieViewModels);
                        }
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
