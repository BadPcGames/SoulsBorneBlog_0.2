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
using UserService= WebApplication1.Services.UserService;
using System.ComponentModel.DataAnnotations;


namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly UserService _userService;
        public AuthController(AppDbContext context, IConfiguration config, UserService userService)
        {
            _context = context;
            _config = config;
            _userService = userService;
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
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginModel model)
        {


            if (model.Password == "" || model.Password == null ||
                 model.Email == "" || model.Password == null )
            {
                return RedirectToAction("Index", new { message = "all data must be fielded" });
            }

            if (!IsValidEmail(model.Email))
            {
                return RedirectToAction("Index", new { message = "email is incorrect" });
            }

            if (!_context.Users.Any(m => m.Email == model.Email))
            {
                return RedirectToAction("Index");
            }

            User user = _context.Users.FirstAsync(m => m.Email == model.Email).Result;
            if (!ShifrService.DeHashPassword(user.PasswordHash, model.Password))
            {
                 return RedirectToAction("Index", new { message = "password is incorrect" });
            }

            await SingIn(user);

            return RedirectToAction("Index", "Profile");
        }

        private async Task SingIn(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.System,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role)
            };
            var claimsIdentyti = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name, ClaimTypes.Role);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentyti);
            await HttpContext.SignInAsync(claimsPrincipal);
        }

        public async Task<IActionResult> SingOut()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
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

            await SingIn(user);

            return RedirectToAction("Index", "Profile");
        }

        public int? GetUserId()
        {
            return _userService.GetUserId();
        }

        public string? GetUserRole()
        {
            return _userService.GetUserRole();
        }

        public byte[]? GetUserAvatar()
        {
            return _userService.GetUserAvatar();
        }

        public async Task<bool> getAccess()
        {
            return _userService.GetAccess().Result;
        }

        public bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        public async Task<IActionResult> GetUsers()
        {
            List<User> users = _context.Users.Where(user=>user.Role=="User").ToList();

            List<UserViewModel> usersViewModel = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                BanTime = user.BanTime,
                Warnings= user.Warnings
            }).ToList();


            if (usersViewModel.Count()==0)
            {
                return Json(new { success = true, message = "Не знайдено доступних користувачів" });
            }

            return Json(usersViewModel);
        }


        public async Task<IActionResult> GetModers()
        {
            List<User> users = _context.Users.Where(user => user.Role == "Moder").ToList();

            List<UserViewModel> usersViewModel = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                BanTime = user.BanTime,
                Warnings = user.Warnings
            }).ToList();


            if (usersViewModel.Count() == 0)
            {
                return Json(new { success = true, message = "Не знайдено доступних модераторів" });
            }

            return Json(usersViewModel);
        }

        public async Task<IActionResult> GetUsersForBunDelete()
        {
            List<User> users = _context.Users.Where(user => user.BanTime!= null).ToList();

            if (users.Count() == 0)
            {
                return Json(new { success = true, message = "Користувачів з обмеженями немає" });
            }

            List<UserViewModel> usersViewModel = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                BanTime = user.BanTime,
                Warnings = user.Warnings
            }).ToList();

            return Json(usersViewModel);
        }
    }
}

