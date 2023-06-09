﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieManagement.ViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieManagement.Controllers
{
    public class LandingPageController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        public LandingPageController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> LandingPage(int limit, int offset, string searchName)
        {
            if (limit <= 0)
            {
                limit = 6;
            }
            if (offset <= 0)
            {
                offset = 0;
            }
            if (searchName == null)
            {
                searchName = "";
            }
            MovieRequestViewModels movieRequestViewModels = new MovieRequestViewModels();
            movieRequestViewModels.Limit = limit;
            movieRequestViewModels.Offset = offset;
            movieRequestViewModels.SearchName = searchName;

            string apiUrl = "https://localhost:7063/api/Movie/allMovie";
            string queryString = $"?limit={movieRequestViewModels.Limit}&offset={movieRequestViewModels.Offset}" +
                $"&searchName={Uri.EscapeDataString(movieRequestViewModels.SearchName)}";

            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl + queryString);
            var responseString = response.Content.ReadAsStringAsync();
            List<MovieViewModels>? movies = new List<MovieViewModels>();
            if (response.IsSuccessStatusCode)
            {
                movies = JsonConvert.DeserializeObject<List<MovieViewModels>>(responseString.Result);
            }
            return View(movies);
        }

        public async Task<IActionResult> Favourite(int limit, int offset, string searchName)
        {
            if (limit <= 0)
            {
                limit = 9;
            }
            if (offset <= 0)
            {
                offset = 0;
            }
            if (searchName == null)
            {
                searchName = "";
            }
            MovieRequestViewModels movieRequestViewModel = new MovieRequestViewModels();
            movieRequestViewModel.Limit = limit;
            movieRequestViewModel.Offset = offset;
            movieRequestViewModel.SearchName = searchName;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                movieRequestViewModel.UserId = user?.Id;
            }
            string apiUrl = "https://localhost:7063/api/movie/getuserfavourite";
            string queryString = $"?limit={movieRequestViewModel.Limit}&offset={movieRequestViewModel.Offset}" +
                $"&searchName={Uri.EscapeDataString(movieRequestViewModel.SearchName)}&UserId={movieRequestViewModel.UserId}";

            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl + queryString);
            var responseString = response.Content.ReadAsStringAsync();
            List<MovieViewModels>? movies = new List<MovieViewModels>();
            if (response.IsSuccessStatusCode)
            {
                movies = JsonConvert.DeserializeObject<List<MovieViewModels>>(responseString.Result);
            }
            ViewBag.Search = movieRequestViewModel.SearchName;
            return View(movies);
        }

        
    }
}