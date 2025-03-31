using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
using WebApplication1;
using Microsoft.AspNetCore.Http;
using WebApplication1.Models;
using WebApplication1.DbModels;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using Google.Apis.Util;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
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

    public async Task<GlobalStatsViewModel> MakeStats(DateTime from, DateTime to)
    {
        GlobalStatsViewModel stats = new GlobalStatsViewModel();
        stats.Name = $"Stats from: {from} to: {to}";

        var posts = _context.Posts.Where(post => post.CreatedAt >= from && post.CreatedAt <= to);

        stats.PostCount = await posts.CountAsync();
        stats.VerifyPostCount = await posts.CountAsync(post => post.Verify);
        stats.NotVerifyPostCount = await posts.CountAsync(post => !post.Verify);

        stats.GameStats = await _context.Games
            .Select(game => new GameStatsViewModel
            {
                Name = game.GameName,
                PostsCount = _context.Posts.Count(post => post.Game == game.GameName)
            }).ToListAsync();


        stats.ThemesStats = await _context.Themes
            .Select(theme => new ThemeStatsViewModel
            {
                Name = theme.Name,
                PostsCount = _context.Posts
                    .Where(post => _context.Blogs
                        .Where(blog => blog.Theme == theme.Name)
                        .Select(blog => blog.Id)
                        .Contains(post.BlogId))
                    .Count()
            }).ToListAsync();

  
        stats.PostShortStats = await posts
            .Select(post => new PostShortStatsViewModel
            {
                Name = post.Title,
                Id = post.Id,
                LikeCount = _context.Reactions.Count(reaction => reaction.Value == 1 && reaction.PostId == post.Id),
                NotLikeCount = _context.Reactions.Count(reaction => reaction.Value == -1 && reaction.PostId == post.Id),
                ComentsCount = _context.Coments.Count(com => com.PostId == post.Id)
            }).ToListAsync();

        return stats;
    }


    public async Task<IActionResult> GetStats(string? TimeStart, string? TimeEnd)
    {
        if ((TimeStart == "" || TimeStart == null) || (TimeEnd == "" || TimeEnd == null))
        {
            return Json(new { success = true, message = "Всі поля мають бути заповені" });
        }

        DateTime from, to;

        if (!DateTime.TryParse(TimeStart, out from))
        {
            return Json(new { success = true, message = "Початковий час вказано не у вірному форматі" });
        }

        if (!DateTime.TryParse(TimeEnd, out to))
        {
            return Json(new { success = true, message = "Остаточний час вказано не у вірному форматі" });
        }

        if (from > to || from > DateTime.Now || to > DateTime.Now)
        {
            return Json(new { success = true, message = "Невірний часовий проміжок" });
        }
        var stats = await MakeStats(from, to);
        return Json(stats);
    }


}
