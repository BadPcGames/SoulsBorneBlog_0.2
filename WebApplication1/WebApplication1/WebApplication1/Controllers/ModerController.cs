using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Xml.Linq;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Models;
using WebApplication1.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

[Authorize(Roles = "Moder,Admin")]
public class ModerController : Controller
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;
    public ModerController(AppDbContext context,EmailService emailService)
    {
        _context = context;
        _emailService=emailService;
    }

    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> TemporaryBan(int userId,string? time)
    {
        if (time == null || time == "")
        {
            return Json(new { success = true, message = "Час не вказано" });
        }
        int banTime = 0;
        if (int.TryParse(time, out banTime))
        {
            User user = _context.Users.First(user => user.Id == userId);
            user.BanTime = DateTime.Now.AddDays(banTime);
            user.Warnings += 1;
            _context.Update(user);
            await _context.SaveChangesAsync();
            _emailService.SendEmail(user.Email, "Тимчасовий бан на платформі SoulsBorneBlogs", "Дорогий користувач вам було надано тимчасове обмеження на додаванян реакцій до публікацій користувачів до: " + user.BanTime);
            return Json(new { success = true, message = "Пользователь успешно забанен!" });
        }
        return Json(new { success = true, message = "Час вказано не вірно" });

    }
    public async Task<IActionResult> DeleteBan(int userId)
    {
        User user = _context.Users.First(user => user.Id == userId);
        user.BanTime = null;
        user.Warnings = user.Warnings>0? user.Warnings - 1:0;
        _context.Update(user);
        await _context.SaveChangesAsync();
        _emailService.SendEmail(user.Email, "Тимчасовий бан на платформі SoulsBorneBlogs", "Дорогий користувач з вас були зняті всі обмеження ");
        return Json(new { success = true, message = "Пользователь успешно разбанен!" });
    }

    public async Task<IActionResult> AbsoluteBanUser(int userId,string reason)
    {
        if (reason == null)
        {
            return Json(new { success = true, message = "Причина не вказана" });
        }
        if (reason.Length < 15)
        {
            return Json(new { success = true, message = "Причина не має бути менше 15 симовлів" });
        }
        User user = _context.Users.First(user => user.Id == userId);
        if (user.Warnings == 0)
        {
            _emailService.SendEmail(user.Email, "Скарга", "Дорогий користувач модератор надсилає вам скаргу з причини "+reason+". Наступна скарга може призвести до видалення вашого акаунту з платформи");
            user.Warnings += 1;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Користувачу надано попередження" });
        }
        else
        {
            var blogs = await _context.Blogs
                .Where(blog => blog.AuthorId == user.Id)
                .ToListAsync();

            if (blogs.Any())
            {
                var postsToDelete = await _context.Posts
                    .Where(p => blogs.Select(b => b.Id).Contains(p.BlogId))
                    .ToListAsync();

                var postIds = postsToDelete.Select(p => p.Id).ToList();

                var contentsToDelete = await _context.Post_Contents
                    .Where(pc => postIds.Contains(pc.PostId))
                    .ToListAsync();

                var reactionsToDelete = await _context.Reactions
                    .Where(r => postIds.Contains(r.PostId))
                    .ToListAsync();

                var commentsToDelete = await _context.Coments
                    .Where(c => postIds.Contains(c.PostId))
                    .ToListAsync();

                _context.Post_Contents.RemoveRange(contentsToDelete);
                _context.Reactions.RemoveRange(reactionsToDelete);
                _context.Coments.RemoveRange(commentsToDelete);
                _context.Posts.RemoveRange(postsToDelete);
                _context.Blogs.RemoveRange(blogs);

                await _context.SaveChangesAsync();
            }

            var userComments = await _context.Coments
                .Where(c => c.AuthorId == userId)
                .ToListAsync();

            if (userComments.Any())
            {
                _context.Coments.RemoveRange(userComments);
                await _context.SaveChangesAsync();
            }

            var userReactions = await _context.Reactions
                .Where(r => r.AuthorId == userId)
                .ToListAsync();

            if (userReactions.Any())
            {
                _context.Reactions.RemoveRange(userReactions);
                await _context.SaveChangesAsync();
            }

            _emailService.SendEmail(
                user.Email,
                "Ви були видалені з платформи SoulsBorneBlogs",
                $"Дорогий користувач, вас видалено з платформи через: {reason}"
            );

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        return Json(new { success = true, message = "Пользователь успешно удалён с платформы!" });
    }
}
