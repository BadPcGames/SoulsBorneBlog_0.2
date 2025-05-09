using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Services;

[Authorize(Roles = "Moder,Admin")]
public class ModerController : Controller
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public ModerController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> TemporaryBan(int userId, string? time)
    {
        if (string.IsNullOrEmpty(time))
        {
            return Json(new { success = true, message = "Ban duration not specified" });
        }

        int banTime = 0;
        if (int.TryParse(time, out banTime))
        {
            if (banTime == 0)
            {
                return Json(new { success = true, message = "Ban duration not specified" });
            }

            User user = _context.Users.First(user => user.Id == userId);
            user.BanTime = DateTime.Now.AddDays(banTime);
            user.Warnings += 1;
            _context.Update(user);
            await _context.SaveChangesAsync();

            _emailService.SendEmail(user.Email,
                "Temporary Ban on SoulsBorneBlogs Platform",
                "Dear user, you have received a temporary restriction on adding reactions to posts until: " + user.BanTime);

            return Json(new { success = true, message = "User successfully banned!" });
        }

        return Json(new { success = true, message = "Invalid ban duration specified" });
    }

    public async Task<IActionResult> DeleteBan(int userId)
    {
        User user = _context.Users.First(user => user.Id == userId);
        user.BanTime = null;
        user.Warnings = user.Warnings > 0 ? user.Warnings - 1 : 0;
        _context.Update(user);
        await _context.SaveChangesAsync();

        _emailService.SendEmail(user.Email,
            "Ban Removed on SoulsBorneBlogs Platform",
            "Dear user, all restrictions have been lifted from your account.");

        return Json(new { success = true, message = "User successfully unbanned!" });
    }

    public async Task<IActionResult> AbsoluteBanUser(int userId, string? reason)
    {
        if (string.IsNullOrEmpty(reason))
        {
            return Json(new { success = true, message = "Reason not specified" });
        }

        if (reason.Length < 15)
        {
            return Json(new { success = true, message = "Reason must be at least 15 characters long" });
        }

        User user = _context.Users.First(user => user.Id == userId);

        if (user.Warnings == 0)
        {
            _emailService.SendEmail(user.Email,
                "Warning",
                "Dear user, a moderator has sent you a warning for the following reason: " + reason +
                ". Further warnings may result in account deletion from the platform.");

            user.Warnings += 1;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Warning issued to user" });
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
                "You Have Been Removed from SoulsBorneBlogs Platform",
                $"Dear user, your account has been deleted from the platform due to the following reason: {reason}"
            );

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, message = "User has been removed from the platform!" });
    }
}
