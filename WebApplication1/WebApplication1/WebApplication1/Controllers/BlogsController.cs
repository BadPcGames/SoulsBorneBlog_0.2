using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Services;

public class BlogsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserService _userService;


    public BlogsController(AppDbContext context,UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // GET: Blogs/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Blogs/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,Theme,AuthorId")] Blog blog,int userId)
    {
        if(blog.Name==""|| blog.Name == null||
            blog.Description == "" || blog.Description == null||
            blog.Theme == "" || blog.Theme == null)
        {
            return View(blog);
        }

        if (ModelState.IsValid)
        {
            blog.AuthorId = userId;
            _context.Add(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Profile");
        }
        return View(blog);
    }

    // GET: Blogs/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Blogs == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs.FindAsync(id);
        if (blog.AuthorId != (int)_userService.GetUserId())
        {
            return RedirectToAction("Index", "Profile");
        }
        if (blog == null)
        {
            return NotFound();
        }
        return View(blog);
    }

    // POST: Blogs/Edit/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Theme")] Blog blog)
    {
        if (id != blog.Id)
        {
            return NotFound();
        }

        if (blog.Name == "" || blog.Name == null ||
          blog.Description == "" || blog.Description == null ||
          blog.Theme == "" || blog.Theme == null)
        {
            return View(blog);
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingBlog = await _context.Blogs.FindAsync(id);
                if (existingBlog == null)
                {
                    return NotFound();
                }

                existingBlog.Name = blog.Name;
                existingBlog.Description = blog.Description;
                existingBlog.Theme = blog.Theme;

                _context.Update(existingBlog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(blog.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index", "Profile");
        }
        return View(blog);
    }

    // GET: Blogs/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.Id == id);
        if (blog == null)
        {
            return NotFound();
        }

        var postsToDelete = await _context.Posts.Where(m => m.BlogId == id).ToListAsync();
        foreach (var post in postsToDelete)
        {
            var contentsToDelete = _context.Post_Contents.Where(m => m.PostId == post.Id);
            _context.Post_Contents.RemoveRange(contentsToDelete);
            var reactionsToDelete = _context.Reactions.Where(m => m.PostId == post.Id);
            _context.Reactions.RemoveRange(reactionsToDelete);
            var comentsToDelete = _context.Coments.Where(m => m.PostId == post.Id);
            _context.Coments.RemoveRange(comentsToDelete);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        _context.Blogs.Remove(blog);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Profile");
    }

    [HttpGet]
    public async Task<IActionResult> GetBlogs()
    {
        var blogs = await _context.Blogs.ToListAsync();
        var users = await _context.Users.ToListAsync();

        var blogsWithAuthors = blogs.Select(blog => new BlogViewModel
        {
            Id = blog.Id,
            Name = blog.Name,
            Description = blog.Description,
            Theme = blog.Theme,
            AuthorName = users.FirstOrDefault(user => user.Id == blog.AuthorId)?.Name ?? "Unknown"
        }).ToList();

        return Json(blogsWithAuthors);
    }

    private bool BlogExists(int id)
    {
        return _context.Blogs.Any(e => e.Id == id);
    }

    [HttpGet]
    public async Task<IActionResult> GetThemes()
    {
        var themes = await _context.Themes.ToListAsync();
        return Json(themes);
    }



    public async Task<IActionResult> GetBlogsForRecomendation(int currentPage, int blogsPerPage,string? blogName)
    {
        int skip = (currentPage - 1) * blogsPerPage;

        var query = _context.Blogs.AsQueryable(); ;

        if (blogName != null)
        {
            query = query.Where(blog => blog.Name.Contains(blogName));
        }

        var blogs = query
            .OrderBy(blog => blog.Id) 
            .Skip(skip)
            .Take(blogsPerPage+1)
            .Select(blog => new BlogViewModel
            {
                Id = blog.Id,
                Name = blog.Name,
                Description = blog.Description,
                Theme = blog.Theme,
                AuthorName = _context.Users
                    .Where(user => user.Id == blog.AuthorId)
                    .Select(user => user.Name)
                    .FirstOrDefault() ?? "Unknown"
            })
            .AsNoTracking();
        return Json(blogs);
    }
}
