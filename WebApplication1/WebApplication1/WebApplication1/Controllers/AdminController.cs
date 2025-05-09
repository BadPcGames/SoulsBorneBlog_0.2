using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Services;




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

    public IActionResult Baner()
    {
        return View();
    }

    public async Task<IActionResult> AddBaner(bool isSelected, IFormFile image)
    {
        if (image == null)
        {
            return Json(new { success = false, message = "Banner not added. Image not selected." });
        }


        var baner = new Baner();


        if (isSelected)
        {
            var baners = _context.Baners.ToList();
            foreach (var b in baners)
            {
                b.Selected = false;
            }
            _context.UpdateRange(baners);
            await _context.SaveChangesAsync();

            baner.Selected = true;
        }

        baner.Image = MyConvert.ConvertFileToByteArray(image);

     
        _context.Baners.Add(baner);  
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Banner added." });
    }
    public async Task<IActionResult> DeleteBaner(int id)
    {
        var baner = _context.Baners.FirstOrDefault(ban => ban.Id == id);

        if (baner.Selected)
        {
            var baners = _context.Baners.ToList();
            baners.First(ban => ban.Id != id).Selected = true;
            _context.UpdateRange(baners);
            await _context.SaveChangesAsync();
        }
        _context.Remove(baner);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Banner deleted" });
    }
    public async Task<IActionResult> SelectBaner(int id)
    {
        var baners = _context.Baners.ToList();

        foreach (var baner in baners)
        {
            baner.Selected = false;
        }

        var selectedBaner = baners.FirstOrDefault(b => b.Id == id);
        if (selectedBaner != null)
        {
            selectedBaner.Selected = true;
        }

        _context.UpdateRange(baners);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Selected banner selected" });
    }

    public async Task<IActionResult> GetBanners()
    {
        var baners = await _context.Baners.ToListAsync();

        if (baners.Count == 0)
        {
            return Json(new { success = true, message = "No banners loaded" });
        }

        var result = baners.Select(b => new
        {
            b.Id,
            Image = b.Image != null ? Convert.ToBase64String(b.Image) : null,
            b.Selected
        });

        return Json(result);
    }

    public async Task<IActionResult> GetHomeBanner()
    {
       var result =_context.Baners.First(ban=>ban.Selected);
       return Json(result);
    }



    public async Task<IActionResult> AddModer(string name,string email)
    {
        if ((name == "") || (email == ""))
        {
            return Json(new { success = true, message = "All fields must be filled in." });
        }

        var user = _context.Users.FirstOrDefault(u => (u.Name == name) && (u.Email == email));
        if (user == null)
        {
            return Json(new { success = true, message = "User with such login and email does not exist" });
        }
        user.Role = "Moder";
        _context.Users.Update(user);
        _context.SaveChanges();
        return Json(new { success = true, message = $"{user.Name} became moder" });
    }

    public async Task<IActionResult> DeleteModer(string Name, string Email)
    {
        if ((Name == "") || (Email == ""))
        {
            return Json(new { success = true, message = "All fields must be filled in." });
        }

        var user = _context.Users.FirstOrDefault(user => user.Name == Name && user.Email == Email&&user.Role=="Moder");
        if (user == null)
        {
            return Json(new { success = true, message = "There is no moderator with such login and email address." });
        }
        user.Role = "User";
        _context.Users.Update(user);
        _context.SaveChanges();
        return Json(new { success = true, message = $"{user.Name} stopped being a moderator" });
    }

}
