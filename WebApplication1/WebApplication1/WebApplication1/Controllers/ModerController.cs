using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Models;

[Authorize(Roles = "Moder,Admin")]
public class ModerController : Controller
{
    private readonly AppDbContext _context;

    public ModerController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> TemporaryBan(int userId,int banTime)
    {
        User user = _context.Users.First(user => user.Id == userId);
        user.BanTime = DateTime.Now.AddDays(banTime);
        _context.Update(user);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Пользователь успешно забанен!" });
    }
    public async Task<IActionResult> DeleteBan(int userId)
    {
        User user = _context.Users.First(user => user.Id == userId);
        user.BanTime = null;
        _context.Update(user);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Пользователь успешно разбанен!" });
    }

 
}
