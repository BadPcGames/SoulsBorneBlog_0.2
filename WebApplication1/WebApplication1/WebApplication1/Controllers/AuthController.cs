using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.DbModels;
using WebApplication1.Models;
using WebApplication1.Services;
using MyConvert = WebApplication1.Services.MyConvert;
using System.ComponentModel.DataAnnotations;


namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Index(string? message)
        {
            ViewBag.Error = message;
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([Bind("Name,Email,Password")] RegisterModel model)
        {

            if (model.Password == "" || model.Password == null ||
                 model.Email == "" || model.Password == null ||
                 model.Name == "" || model.Name == null)
            {
                return RedirectToAction("Index", new { message = "all data must be fielded" });
            }

            if (!IsValidEmail(model.Email))
            {
                return RedirectToAction("Index", new { message = "email is incorrect" });
            }

            if (!_context.Users.Any(m => m.Email == model.Email))
            {
                if (model.Password.Length < 6)
                {
                    return RedirectToAction("Index", new { message = "password is too short" });
                }

                using (var stream = System.IO.File.OpenRead("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png"))
                {
                    User user = new User
                    {
                        Name = model.Name,
                        Email = model.Email,
                        PasswordHash = ShifrService.HashPassword(model.Password),
                        Role = "User",
                        Avatar = MyConvert.ConvertFileToByteArray(new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name)))
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    Login(new LoginModel()
                    {
                        Email = model.Email,
                        Password = model.Password
                    });
                }
                return RedirectToAction("Index", "Profile");
            }
            else
            {
                return RedirectToAction("Index",new { message = "email is alredy used" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.Email))
            {
                return RedirectToAction("Index", new { message = "all data must be fielded" });
            }

            if (!IsValidEmail(model.Email))
            {
                return RedirectToAction("Index", new { message = "email is incorrect" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == model.Email);
            if (user == null)
            {
                return RedirectToAction("Index", new { message = "no account found with this email" });
            }

            if (!ShifrService.DeHashPassword(user.PasswordHash, model.Password))
            {
                return RedirectToAction("Index", new { message = "password is incorrect" });
            }

            return await SignIn(user);
        }

        public async Task<IActionResult> SignIn(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name, ClaimTypes.Role);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Profile");
        }


        [Authorize]
        public async Task<IActionResult> EditProfileData(string? Name, string? Email, string? Password)
        {
            var user = _context.Users.First(user => user.Id == int.Parse(HttpContext.User.FindFirst(ClaimTypes.System).Value));

            if (Name != null)
            {
                user.Name = Name;
            }
            if (Email != null)
            {
                user.Email = Email;
            }
            if (Password != null)
            {
                user.PasswordHash = ShifrService.HashPassword(Password);
            }
            _context.Update(user);
            await _context.SaveChangesAsync();

            return await SignIn(user);
        }


        public async Task<IActionResult> SingOut()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public int? GetUserId()
        {
            if (HttpContext.User.FindFirst(ClaimTypes.System)?.Value!=null)
            {
                return int.Parse(HttpContext.User.FindFirst(ClaimTypes.System)?.Value);
            }
            return null; 
        }

        public string? GetUserRole()
        {
            if (GetUserId != null)
            {
                return HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            }
            return null;
        }

        public byte[]? GetUserAvatar()
        {
            if (GetUserId() != null)
            {
                if (_context.Users.First(user => user.Id == GetUserId()).Avatar != null)
                {
                    return _context.Users.First(user => user.Id == GetUserId()).Avatar;
                }
            }
            return null;
        }

        public async Task<bool> getAccess()
        {
            User user=_context.Users.First(user=>user.Id==GetUserId());
            if (user.BanTime == null) return true;
            Console.WriteLine(user.BanTime);
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(user.BanTime < DateTime.Now);
            if (user.BanTime < DateTime.Now)
            {
                user.BanTime = null;
                _context.Update(user);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }
    }
}

