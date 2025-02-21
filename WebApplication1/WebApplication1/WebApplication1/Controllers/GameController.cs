using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Models;
using WebApplication1.Services;

[Authorize(Roles = "Admin")]
public class GameController : Controller
{
    private readonly AppDbContext _context;

    public GameController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        List<Game> games=_context.Games.ToList();
        return View(games);
    }
  
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string GameName,string Description,IFormFile GameCharacter,string Color)
    {
        Console.WriteLine(GameCharacter);

        Game game = new Game()
        {
            GameName = GameName,
            Description = Description,
            GameCharacter=MyConvert.ConvertFileToByteArray(GameCharacter),
            Color = Color
        };
        _context.Add(game);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }


    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Blogs == null)
        {
            return NotFound();
        }
        var game = await _context.Games.FindAsync(id);
        if (game == null)
        {
            return NotFound();
        }
        return View(game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,string GameName, string Description, IFormFile? GameCharacter, string Color)
    {
        var existingGame = await _context.Games.FindAsync(id);
        if (existingGame == null)
        {
            return NotFound();
        }
        existingGame.GameName = GameName;
        existingGame.Description = Description;
        if(GameCharacter!=null) existingGame.GameCharacter =  MyConvert.ConvertFileToByteArray(GameCharacter);
        existingGame.Color = Color;
        _context.Update(existingGame);
        await _context.SaveChangesAsync();
       
        return RedirectToAction("Index");

    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var game=await _context.Games.FirstOrDefaultAsync(m => m.Id == id);
        if (game == null)
        {
            return NotFound();
        }

        var postsToDelete = await _context.Posts.Where(m => m.Game == game.GameName).ToListAsync();
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

        _context.Games.Remove(game);

        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> GetGame(string gameName)
    {
        var game = _context.Games.First(game => game.GameName == gameName);
        GameViewModel gameData=new GameViewModel()
        {
            GameName=game.GameName,
            GameCharacter=game.GameCharacter,
            Description=game.Description,
            Color=game.Color,
        };
        return Json(gameData);
    }

    [HttpGet]
    public async Task<IActionResult> GetGames()
    {
        var games = _context.Games.ToList();
        List<GameViewModel> gamesData = games.Select(game => new GameViewModel()
        {
            GameName = game.GameName,
            GameCharacter = game.GameCharacter,
            Description = game.Description,
            Color = game.Color,
        }).ToList();
        return Json(gamesData);
    }
}
