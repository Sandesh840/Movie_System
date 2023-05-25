using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VideoStream.Data;
using VideoStream.Models;
using VideoStream.VIewModels;

namespace VideoStream.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext application,UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _applicationDbContext = application;  
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }
        public async Task<IActionResult> SignUpSave(RegisterViewModels registerViewModels)
        {
            var user = new IdentityUser()
            {
                UserName = registerViewModels.Name,
                Email = registerViewModels.Email,
            };
            var result =await _userManager.CreateAsync(user,registerViewModels.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return RedirectToAction("SignUp");
            }
            return RedirectToAction("Index","Home");
        }
    }
}