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
        private readonly UserService _userService;
        public ProfileController(AppDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IActionResult> Index(int? id)
        {
            int currentUserId = (int)_userService.GetUserId();

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

        public IActionResult Stats()
        {
            return View(_userService.GetUserId());
        }

        public async Task<IActionResult> Edit(string password)
        {
            var user = _context.Users.First(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value));
            if (ShifrService.HashPassword(password) == user.PasswordHash)
            {
                return View(user);
            }
            return RedirectToAction("Index", "Profile");
        }

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

