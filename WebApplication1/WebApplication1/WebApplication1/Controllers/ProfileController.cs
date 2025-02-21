using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Services;
using MyConvert = WebApplication1.Services.MyConvert;


namespace WebApplication1.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        public ProfileController(AppDbContext context, IConfiguration config)
        {
            _context = context;
        }


        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = _context.Users.First(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value));

            ViewBag.User = new UserViewModel()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar
            };
            var yourBlogs = _context.Blogs.Where(blog => blog.AuthorId ==
                int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value)).ToList();
            return View(yourBlogs);
        }

        [Authorize]
        public async Task<IActionResult> Edit(string password)
        {
            var user = _context.Users.First(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value));
            if (ShifrService.HashPassword(password) == user.PasswordHash)
            {
                return View(user);
            }
            return RedirectToAction("Index", "Profile");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangeAvatar(IFormFile? Avatar)
        {
            var user = _context.Users.First(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value));
            byte[] avatar = Avatar != null ? MyConvert.ConvertFileToByteArray(Avatar) : null;
            user.Avatar = avatar;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Profile");
        }


    }
}

