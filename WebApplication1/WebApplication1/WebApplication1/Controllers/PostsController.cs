using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using WebApplication1.Models;
using System.Text;
using WebApplication1.Services;



public class PostsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;
    private readonly EmailService _emailService;
    public PostsController(AppDbContext context, UserService userService, IConfiguration configuration)
    {
        _context = context;
        _userService = userService;
        _configuration = configuration;
        _emailService = new EmailService();
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
            Verify = post.Verify
        }).ToList();

        if (postViewModels.Count() == 0)
        {
            return Json(new { success = true, message = "Публікацій не знайдено" });
        }

        return Json(postViewModels);
    }

    public IActionResult Stats(int postId)
    {
        return View(postId);
    }

    public async Task<IActionResult> PostApproval(int postId)
    {
        var post=_context.Posts.FirstOrDefault(post => post.Id == postId);
        post.Verify = true;
        _context.Update(post);
        _context.SaveChanges();

        User user = _context.Users.FirstOrDefault(user => user.Id == _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).AuthorId);
        _emailService.SendEmail(user.Email,"Публікація "+post.Title,"Була одобрена та опублікована");

       return Json(new { success = true, message = "Публікація схвалена" });
    }

    public async Task<IActionResult> PostNotApproval(int postId, string? reason)
    {
        if (reason==null||reason=="")
        {
            return Json(new { success = true, message = "Причина не написана" });
        }
        if (reason.Length<15)
        {
            return Json(new { success = true, message = "Опиши причину детальніше" });
        }

        var post = _context.Posts.FirstOrDefault(post => post.Id == postId);
        User user = _context.Users.FirstOrDefault(user => user.Id == _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).AuthorId);
        _emailService.SendEmail(user.Email, "Публікація " + post.Title, "Була одобрена скрита з причини:" +reason);

        return Json(new { success = true, message = "Повідомлення відправлено" });
    }

    // GET: Posts
    public async Task<IActionResult> Index(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid blog ID.");
        }

        ViewBag.BlogId = id;
        var posts = await _context.Posts
                                  .Where(post => post.BlogId == id)
                                  .ToListAsync();
        if (_userService.GetUserId() != null)
        {
            ViewBag.CanChange= _userService.GetUserId() == _context.Blogs.First(blog => blog.Id == id).AuthorId;
        }
        else
        {
            ViewBag.CanChange=false;
        }

        var postsToShow = posts.Select(post => new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            BlogId = post.BlogId,
            Game = post.Game,
            CreateAt=post.CreatedAt,
            AuthorName = _context.Users.FirstOrDefault(
                user => user.Id == _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).AuthorId)?.Name ?? "Unknown",
            BlogName=_context.Blogs.FirstOrDefault(blog=>blog.Id==post.BlogId)?.Name ?? "Unknown",
            Contents=_context.Post_Contents.Where(postsContents=>postsContents.PostId==post.Id).ToList()
        }).ToList();

        ViewBag.BlogName = _context.Blogs.FirstOrDefault(blog => blog.Id == id).Name;
        return View(postsToShow);
    }

    // GET: One Post
    public async Task<IActionResult> ReadPost(int id)
    {
        return View(getPost(id));
    }

    private PostViewModel getPost(int id)
    {
        var post = _context.Posts.FirstOrDefault(post => post.Id == id);
        PostViewModel postToShow = new PostViewModel()
        {
            Id = post.Id,
            Title = post.Title,
            BlogId = post.BlogId,
            Game = post.Game,
            CreateAt = post.CreatedAt,
            AuthorName = _context.Users.FirstOrDefault(
                    user => user.Id == _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).AuthorId)?.Name ?? "Unknown",
            BlogName = _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId)?.Name ?? "Unknown",
            Contents = _context.Post_Contents.Where(postsContents => postsContents.PostId == post.Id).ToList()
        };
        return postToShow;
    }

    // GET: Posts/Create/5
    public IActionResult Create(int blogId,string? message)
    {
        ViewBag.Error = message;
        if (_context.Posts.Where(post => post.BlogId == blogId).Count() >= 50)
        {
            return RedirectToAction("Index", new { id = blogId });
        }
        var model = new Post
        {
            BlogId = blogId
        };
        return View(model);
    }




    // POST: Posts/Create/5
    [HttpPost]
    public async Task<IActionResult> Create(int blogId, [Bind("Title,Game")] Post post, List<PostContentViewModel> contents)
    {
        if (post.Title == "" || post.Game == "")
        {
            return RedirectToAction("Create", new { id = blogId, message = "all data must be fuiled" });
        }
        post.BlogId = blogId;
        post.CreatedAt = DateTime.Now;
        post.Verify = false;
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        if (contents != null)
        {
            foreach (var content in contents)
            {
                var contentData= new byte[0];
                switch (content.ContentType)
                {
                    case "Text":
                        contentData = Encoding.UTF8.GetBytes(content.Content);
                        break;
                    default:
                        contentData = content.FormFile != null ? MyConvert.ConvertFileToByteArray(content.FormFile) : null;
                        break;
                }
               
                _context.Post_Contents.Add(new Post_Content
                {
                    PostId = post.Id,
                    ContentType = content.ContentType,
                    Content = contentData,
                    Position = content.Position
                });
                await _context.SaveChangesAsync();
            } 
        }

        return RedirectToAction("Index", new { id = blogId });
    }

    public IActionResult Edit(int id, string? message)
    {
        ViewBag.Error = message;
        return View(getPost(id));
    }

    [HttpPost]
    public async Task<IActionResult> Edit([Bind("Id,Title,Game")] Post post, List<PostContentViewModel> contents)
    {
        if (post.Title == "" || post.Game == "")
        {
            return RedirectToAction("Edit", new { id = post.Id, message = "all data must be fuiled" });
        }

        var oldPost = _context.Posts.First(p=>p.Id==post.Id);

        if (oldPost.Title != post.Title)
        {
            oldPost.Title = post.Title;
        }
        if (oldPost.Game != post.Game)
        {
            oldPost.Game = post.Game;
        }

        oldPost.Verify = false;
        _context.Posts.Update(oldPost);
        await _context.SaveChangesAsync();

        var oldContent = _context.Post_Contents.Where(con => con.PostId == post.Id);

        foreach (var content in oldContent)
        {
            _context.Post_Contents.Remove(content);
        }
        await _context.SaveChangesAsync();

        if (contents != null)
        {
            foreach (var content in contents)
            {
                var contentData = new byte[0];
                switch (content.ContentType)
                {
                    case "Text":
                        contentData = Encoding.UTF8.GetBytes(content.Content);
                        break;
                    default:
                        //contentData = content.FormFile != null ? MyConvert.ConvertFileToByteArray(content.FormFile) : null;
                        contentData = content.Content != null ? Convert.FromBase64String(content.Content) : null;
                        break;
                }

                _context.Post_Contents.Add(new Post_Content
                {
                    PostId = post.Id,
                    ContentType = content.ContentType,
                    Content = contentData,
                    Position = content.Position
                });
                await _context.SaveChangesAsync();
            }
        }

        return RedirectToAction("Index", new { id = oldPost.BlogId });
    }
    // POST: Posts/Dlete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
        if (post == null)
        {
            return NotFound();
        }

        int blogId = post.BlogId;

        var contentsToDelete = _context.Post_Contents.Where(m => m.PostId == id);
        _context.Post_Contents.RemoveRange(contentsToDelete);
        var reactionsToDelete = _context.Reactions.Where(m => m.PostId == id);
        _context.Reactions.RemoveRange(reactionsToDelete);
        var comentsToDelete = _context.Coments.Where(m => m.PostId == id);
        _context.Coments.RemoveRange(comentsToDelete);
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { id = blogId });
    }

    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = await _context.Games.ToListAsync();
        return Json(games);
    }

    [HttpGet]
    public async Task<IActionResult> GetLikes(int postId)
    {
        int likeCount = await _context.Reactions
                                      .Where(like => like.PostId == postId && like.Value > 0)
                                      .CountAsync();
        return Ok(likeCount);
    }

    [HttpGet]
    public async Task<IActionResult> GetDisLikes(int postId)
    {
        int likeCount = await _context.Reactions
                                      .Where(like => like.PostId == postId && like.Value < 0)
                                      .CountAsync();
        return Ok(likeCount);
    }

    [HttpPost]
    public async Task<IActionResult> MakeReactions(int value, int postId)
    {
        if (HttpContext == null|| _userService.GetUserId()==null)
        {
            return Unauthorized(new { redirectUrl = "/Auth/Index" });
        }
        bool access = await _userService.GetAccess();
        if (!access)
        {
            var user = _context.Users.FirstOrDefault(user => user.Id == _userService.GetUserId());
            return Json(new { success = true, message = $"Ви знаходетесь в бані до {user.BanTime}" }) ;
        }

        int clientId = (int)_userService.GetUserId();

        Reactions? existingReaction = _context.Reactions
                                              .FirstOrDefault(reaction => reaction.AuthorId == clientId && reaction.PostId == postId);

        if (existingReaction == null)
        {
            Reactions reactions = new Reactions()
            {
                Value = value,
                AuthorId = clientId,
                PostId = postId
            };

            _context.Add(reactions);
            await _context.SaveChangesAsync();
        }
        else
        {
            if (existingReaction.Value != value)
            {
                existingReaction.Value = value;
                _context.Update(existingReaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Reactions.Remove(existingReaction);
                await _context.SaveChangesAsync();
            }
        }
        return Json(new { success = true, message = "" });
    }

    [HttpPost]
    public async Task<IActionResult> MakeComent(string text,int postId)
    {
        if (HttpContext.User == null|| _userService.GetUserId()==null)
        {
            return Unauthorized(new { redirectUrl = "/Auth/Index" });
        }
        bool access = await _userService.GetAccess();
        if (!access)
        {
            var user = _context.Users.FirstOrDefault(user => user.Id == _userService.GetUserId());
            return Json(new { success = true, message = $"Ви знаходетесь в бані до {user.BanTime}" });
        }
        if (text == "")
        {
            return Json(new { success = true, message = "Коментар не може бути пустим" });
        }
        int clientId = (int)_userService.GetUserId();
        Coments newComent = new Coments()
        {
            AuthorId = clientId,
            Text= text,
            PostId= postId,
            CreateAt = DateTime.Now
        };

       _context.Coments.Add(newComent);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "" });
    }

    [HttpGet]
    public async Task<IActionResult> GetComents(int postId)
    {
        var coments = await _context.Coments.Where(coment => coment.PostId == postId).ToListAsync();
        var users = await _context.Users.ToListAsync();
        List<ComentViewModel> comentsToShow = coments.Select(coment => new ComentViewModel
        {
            Id = coment.Id,
            Text = coment.Text,
            PostId = coment.PostId,
            CanChange = coment.CreateAt.AddMinutes(15) > DateTime.Now,
            AuthorId = coment.AuthorId,
            AuthorName = users.FirstOrDefault(user => user.Id == coment.AuthorId).Name,
            AuthorAvatar = users.FirstOrDefault(user => user.Id == coment.AuthorId).Avatar
        }).ToList();
        return Json(comentsToShow);
    }

    //EditComment
    public async Task<IActionResult> EditComment(int id,string text)
    {
        if (HttpContext.User == null || _userService.GetUserId() == null)
        {
            return Unauthorized(new { redirectUrl = "/Auth/Index" });
        }
        bool access = await _userService.GetAccess();
        if (!access)
        {
            var user = _context.Users.FirstOrDefault(user => user.Id == _userService.GetUserId());
            return Json(new { success = true, message = $"Ви знаходетесь в бані до {user.BanTime}" });
        }
        if (text == "")
        {
            return Json(new { success = true, message = "Коментар не може бути пустим" });
        }
        var existingComment = await _context.Coments.FindAsync(id);
        if (existingComment == null)
        {
            return NotFound();
        }

        existingComment.Text= text;

        _context.Update(existingComment);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        if (HttpContext.User == null || _userService.GetUserId() == null)
        {
            return Unauthorized(new { redirectUrl = "/Auth/Index" });
        }


        if (commentId == null)
        {
            return NotFound();
        }

        var comment = await _context.Coments.FirstOrDefaultAsync(m => m.Id == commentId);
        if (comment == null)
        {
            return NotFound();
        }
        _context.Coments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok();
    }

    public async Task<IActionResult> GetPosts(int currentPage, int postsPerPage,string? filterGame = null, string? filterTheme = null,string? postName=null)
    {
        var query = _context.Posts.AsQueryable();
        int skip = (currentPage - 1) * postsPerPage;

        query = query.Where(post => post.Verify);

        if (!string.IsNullOrEmpty(postName))
        {
            query = query.Where(post => post.Title.Contains(postName));
        }

        if (!string.IsNullOrEmpty(filterGame))
        {
            query = query.Where(post => post.Game == filterGame);
        }

        if (!string.IsNullOrEmpty(filterTheme))
        {
            query = query.Where(post => _context.Blogs
                .Where(blog => blog.Id == post.BlogId)
                .Select(blog => blog.Theme)
                .FirstOrDefault() == filterTheme);
        }

        List<Post> posts= await query
            .OrderByDescending(post => post.CreatedAt) 
            .Skip(skip) 
            .Take(postsPerPage) 
            .ToListAsync();

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
            Contents = _context.Post_Contents.Where(postContents => postContents.PostId == post.Id).ToList()
        }).ToList();

        return Json(postViewModels);
    }
}
