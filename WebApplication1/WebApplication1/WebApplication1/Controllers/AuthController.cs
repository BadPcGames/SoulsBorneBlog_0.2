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
using UserService = WebApplication1.Services.UserService;
using System.ComponentModel.DataAnnotations;
using MimeKit.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;


namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;
        private IConfiguration _config;
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly AdminEmailOptions _adminEmail;
        private readonly IMemoryCache _memoryCache;

        public AuthController(
            AppDbContext context,
            IConfiguration config,
            UserService userService,
            EmailService emailService,
            IOptions<AdminEmailOptions> adminEmailOptions,
            IMemoryCache memoryCache)
        {
            _context = context;
            _config = config;
            _userService = userService;
            _emailService = emailService;
            _adminEmail = adminEmailOptions.Value;
            _memoryCache = memoryCache;
        }

        [Authorize]
        public IActionResult Report()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] EmailRequestModel model)
        {
            if (!IsValidEmail(model.Email))
                return Json(new { success = false, message = "Invalid email address" });

            var token = Guid.NewGuid().ToString();
            var cacheKey = $"email_confirm_{model.Email}";
            _memoryCache.Set(cacheKey, token, TimeSpan.FromMinutes(15));

            var confirmUrl = Url.Action("ConfirmEmail", "Auth", new { email = model.Email, token = token }, protocol: Request.Scheme);

            var body = $@"
            <p>To confirm your email, click the button below:</p>
            <p><a href='{confirmUrl}' style='display:inline-block;padding:10px 15px;background-color:#28a745;color:white;text-decoration:none;border-radius:5px;'>Confirm Email</a></p>
            <p>Or copy and paste this link into your browser:</p>
            <p>{confirmUrl}</p>";

            try
            {
                await _emailService.SendHtmlEmailAsync(model.Email, "Email Confirmation", body);
                return Json(new { success = true, message = "Confirmation has been sent to the specified email.", token = token });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error sending email: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string email, string token)
        {
            var cacheKey = $"email_confirm_{email}";
            var verifiedKey = $"email_verified_{email}";

            if (_memoryCache.TryGetValue(verifiedKey, out bool isVerified) && isVerified)
            {
                ViewBag.Email = email;
                return View("ConfirmEmailResult");
            }

            if (_memoryCache.TryGetValue(cacheKey, out string storedToken) && storedToken == token)
            {
                _memoryCache.Set(verifiedKey, true, TimeSpan.FromMinutes(30));
                ViewBag.Email = email;
                return View("ConfirmEmailResult");
            }

            return Content("Invalid or expired link");
        }

        [HttpPost]
        public IActionResult ClearEmailConfirmationCache([FromBody] EmailRequestModel request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { success = false, message = "Invalid email." });

            var confirmKey = $"email_confirm_{request.Email}";
            var verifiedKey = $"email_verified_{request.Email}";

            _memoryCache.Remove(confirmKey);
            _memoryCache.Remove(verifiedKey);

            return Ok(new { success = true });
        }

        public class EmailRequestModel
        {
            public string Email { get; set; }
        }

        [Authorize]
        public async Task<IActionResult> MakeReport(string topic, string text)
        {
            var user = _context.Users.FirstOrDefault(user => user.Id == _userService.GetUserId());

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(topic))
            {
                return Json(new { success = true, message = "All fields must be filled in" });
            }

            _emailService.SendEmail(_adminEmail.Email, $"Complaint on topic {topic}", $"User \"{user.Name}\" \nEmail \"{user.Email}\" \n{text}");

            return Json(new { success = true, message = "Complaint has been sent" });
        }

        public IActionResult Index(string? message)
        {
            ViewBag.Error = message;
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([Bind("Name,Email,Password")] RegisterModel model)
        {
            if (_context.Users.Count() >= 5000)
            {
                return RedirectToAction("Index", new { message = "Too many users" });
            }

            if (string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Name))
            {
                return RedirectToAction("Index", new { message = "All fields must be filled in" });
            }

            if (!IsValidEmail(model.Email))
            {
                return RedirectToAction("Index", new { message = "Invalid email" });
            }

            if (!_context.Users.Any(m => m.Email == model.Email))
            {
                if (model.Password.Length < 6)
                {
                    return RedirectToAction("Index", new { message = "Password is too short" });
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
                return RedirectToAction("Index", new { message = "Email is already in use" });
            }
        }

        public async Task<IActionResult> Login([Bind("Email,Password")] LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.Email))
            {
                return RedirectToAction("Index", new { message = "All fields must be filled in" });
            }

            if (!IsValidEmail(model.Email))
            {
                return RedirectToAction("Index", new { message = "Invalid email" });
            }

            if (!_context.Users.Any(m => m.Email == model.Email))
            {
                return RedirectToAction("Index");
            }

            User user = _context.Users.FirstAsync(m => m.Email == model.Email).Result;
            if (!ShifrService.DeHashPassword(user.PasswordHash, model.Password))
            {
                return RedirectToAction("Index", new { message = "Incorrect password" });
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
            List<User> users = _context.Users.Where(user => user.Role == "User").ToList();

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
                return Json(new { success = true, message = "No available users found" });
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
                return Json(new { success = true, message = "No available moderators found" });
            }

            return Json(usersViewModel);
        }

        public async Task<IActionResult> GetUsersForBunDelete()
        {
            List<User> users = _context.Users.Where(user => user.BanTime != null).ToList();

            if (users.Count() == 0)
            {
                return Json(new { success = true, message = "There are no users with restrictions" });
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
