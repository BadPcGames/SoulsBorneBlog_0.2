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
        public async Task<IActionResult> Index(int? id)
        {
            int currentUserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value);

            if (id == null || id == currentUserId)
            {
                var user = _context.Users.First(u => u.Id == currentUserId);

                ViewBag.User = new UserViewModel()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Avatar = user.Avatar
                };

                var yourBlogs = _context.Blogs.Where(blog => blog.AuthorId == currentUserId).ToList();
                ViewBag.CanChange = true;
                return View(yourBlogs);
            }

            var otherUser = _context.Users.FirstOrDefault(u => u.Id == id);
            if (otherUser == null)
            {
                return NotFound(); 
            }

            ViewBag.User = new UserViewModel()
            {
                Id = otherUser.Id,
                Name = otherUser.Name,
                Email = otherUser.Email,
                Avatar = otherUser.Avatar
            };

            var otherBlogs = _context.Blogs.Where(blog => blog.AuthorId == id).ToList();
            ViewBag.CanChange = false;
            return View(otherBlogs);
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

