using Microsoft.AspNetCore.Mvc;
using WebApplication1;




public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Stats()
    {
        return View();
    }

    public async Task<IActionResult> AddModer(string name,string email)
    {
        var user = _context.Users.FirstOrDefault(u => (u.Name == name) && (u.Email == email));
        if (user == null)
        {
            return Json(new { success = true, message = "Користувача з такими логіном та поштою не існує" });
        }
        user.Role = "Moder";
        _context.Users.Update(user);
        _context.SaveChanges();
        return Json(new { success = true, message = $"{user.Name} став модератором" });
    }

    public async Task<IActionResult> DeleteModer(string Name, string Email)
    {
        var user = _context.Users.FirstOrDefault(user => user.Name == Name && user.Email == Email&&user.Role=="Moder");
        if (user == null)
        {
            return Json(new { success = true, message = "Модератора з такими логіном та поштою не існує" });
        }
        user.Role = "User";
        _context.Users.Update(user);
        _context.SaveChanges();
        return Json(new { success = true, message = $"{user.Name} перестав бути модератором" });
    }

}
