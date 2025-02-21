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

    public async Task<IActionResult> GetPostsToVerify()
    {
        List<Post> posts = _context.Posts.Where(post => post.Verify != true).ToList();

        List<PostViewModel> postViewModels = posts.Select(post => new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            CreateAt = post.CreatedAt,
            Game = post.Game,
            Color = _context.Games.First(game => game.GameName == post.Game).Color,
            BlogId = post.BlogId,
            BlogName = _context.Blogs.Where(blog => blog.Id == post.BlogId).Select(blog => blog.Name).FirstOrDefault() ?? "Unknown",
            AuthorName = _context.Users.Where(user => user.Id == _context.Blogs.Where(blog => blog.Id == post.BlogId).Select(blog => blog.AuthorId).FirstOrDefault())
                               .Select(user => user.Name).FirstOrDefault() ?? "Unknown",
            Contents = _context.Post_Contents.Where(postContents => postContents.PostId == post.Id).ToList(),
            Verify=post.Verify
        }).ToList();

        return Json(postViewModels);
    }
    public async Task<IActionResult> GetUsers()
    {
        List<User> users =_context.Users.ToList();

        List<UserViewModel> usersViewModel = users.Select(user => new UserViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Avatar=user.Avatar,
            BanTime=user.BanTime
        }).ToList();

        return Json(usersViewModel);
    }

    public async Task<IActionResult> TemporaryBan(int userId,int banTime)
    {
        User user = _context.Users.First(user => user.Id == userId);
        user.BanTime = DateTime.Now.AddDays(banTime);
        _context.Update(user);
        await _context.SaveChangesAsync();
        return Ok();
    }
    public async Task<IActionResult> DeleteBan(int userId)
    {
        User user = _context.Users.First(user => user.Id == userId);
        user.BanTime = null;
        _context.Update(user);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
