using Microsoft.AspNetCore.Mvc;
using WebApplication1;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using QuestPDF.Helpers;
using QuestPDF.Fluent;

public class StatsController : Controller
{
    private readonly AppDbContext _context;

    public StatsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StatsViewModel> GenerateGlobalStats(DateTime from, DateTime to)
    {
        if (to.Day == DateTime.Now.Day)
        {
            to = DateTime.Now;
        }
        if (to.Day < DateTime.Now.Day)
        {
            to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 0);
        }

        StatsViewModel stats = new StatsViewModel
        {
            Name = $"Stats from: {from} to: {to}"
        };

        var posts = _context.Posts.Where(post => post.CreatedAt >= from && post.CreatedAt <= to);

        stats.PostCount = await posts.CountAsync();
        stats.VerifyPostCount = await posts.CountAsync(post => post.Verify);
        stats.NotVerifyPostCount= await posts.CountAsync(post => !post.Verify);

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
                ComentsCount = _context.Coments.Count(com => com.PostId == post.Id),
                BlogName = _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).Name,
                Verify = post.Verify,
                CreateAt = post.CreatedAt
            }).ToListAsync();

        return stats;
    }

    public async Task<PostShortStatsViewModel> GeneratePostStats(int postId)
    {
        var post = _context.Posts.FirstOrDefault(p => p.Id == postId);

        return new PostShortStatsViewModel
        {
            Name = post.Title,
            Id = post.Id,
            LikeCount = _context.Reactions.Count(r => r.Value == 1 && r.PostId == post.Id),
            NotLikeCount = _context.Reactions.Count(r => r.Value == -1 && r.PostId == post.Id),
            ComentsCount = _context.Coments.Count(c => c.PostId == post.Id),
            BlogName = _context.Blogs.FirstOrDefault(b => b.Id == post.BlogId).Name,
            Verify = post.Verify,
            CreateAt = post.CreatedAt
        };
    }

    public async Task<List<PostShortStatsViewModel>> GenerateUserStats(DateTime from, DateTime to, int userId)
    {
        if (to.Day == DateTime.Now.Day)
        {
            to = DateTime.Now;
        }
        if (to.Day < DateTime.Now.Day)
        {
            to = new DateTime(to.Year, to.Month, to.Day, 23, 59, 0);
        }

        var blogIds = _context.Blogs.Where(blog => blog.AuthorId == userId).Select(blog => blog.Id);

        var posts = _context.Posts.Where(post => post.CreatedAt >= from && post.CreatedAt <= to && blogIds.Contains(post.BlogId));

        return await posts
            .Select(post => new PostShortStatsViewModel
            {
                Name = post.Title,
                Id = post.Id,
                LikeCount = _context.Reactions.Count(r => r.Value == 1 && r.PostId == post.Id),
                NotLikeCount = _context.Reactions.Count(r => r.Value == -1 && r.PostId == post.Id),
                ComentsCount = _context.Coments.Count(c => c.PostId == post.Id),
                BlogName = _context.Blogs.FirstOrDefault(b => b.Id == post.BlogId).Name,
                Verify = post.Verify,
                CreateAt = post.CreatedAt
            }).ToListAsync();
    }

    public async Task<IActionResult> GetUserStats(string? timeStart, string? timeEnd, int userId)
    {
        if (string.IsNullOrWhiteSpace(timeStart) || string.IsNullOrWhiteSpace(timeEnd))
            return Json(new { success = true, message = "All fields must be filled in" });

        if (!DateTime.TryParse(timeStart, out var from))
            return Json(new { success = true, message = "Start time is in an invalid format" });

        if (!DateTime.TryParse(timeEnd, out var to))
            return Json(new { success = true, message = "End time is in an invalid format" });

        if (from > to || from > DateTime.Now || to > DateTime.Now)
            return Json(new { success = true, message = "Invalid time range" });

        var stats = await GenerateUserStats(from, to, userId);

        if (!stats.Any())
            return Json(new { success = true, message = "No statistics available" });

        return Json(stats);
    }

    public async Task<IActionResult> GetPostStats(int postId)
    {
        var stats = await GeneratePostStats(postId);
        return Json(stats);
    }

    public async Task<IActionResult> GetGlobalStats(string? timeStart, string? timeEnd)
    {
        if (string.IsNullOrWhiteSpace(timeStart) || string.IsNullOrWhiteSpace(timeEnd))
            return Json(new { success = true, message = "All fields must be filled in" });

        if (!DateTime.TryParse(timeStart, out var from))
            return Json(new { success = true, message = "Start time is in an invalid format" });

        if (!DateTime.TryParse(timeEnd, out var to))
            return Json(new { success = true, message = "End time is in an invalid format" });

        if (from > to || from > DateTime.Now || to > DateTime.Now)
            return Json(new { success = true, message = "Invalid time range" });

        var stats = await GenerateGlobalStats(from, to);
        return Json(stats);
    }

    public async Task<IActionResult> GetFileStats(string? timeStart, string? timeEnd, string type)
    {
        if (string.IsNullOrEmpty(timeStart) || string.IsNullOrEmpty(timeEnd))
            return Json(new { success = false, message = "All fields must be filled in" });

        if (!DateTime.TryParse(timeStart, out var from))
            return Json(new { success = false, message = "Start time is in an invalid format" });

        if (!DateTime.TryParse(timeEnd, out var to))
            return Json(new { success = false, message = "End time is in an invalid format" });

        if (from > to || from > DateTime.Now || to > DateTime.Now)
            return Json(new { success = false, message = "Invalid time range" });

        var stats = await GenerateGlobalStats(from, to);

        return type == "ex" ? await DownloadExcel(stats) : await DownloadPdf(stats);
    }

    public async Task<IActionResult> DownloadExcel(StatsViewModel model)
    {
        using var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Report");

        int rowIndex = 0;

        rowIndex = AddTextRow(sheet, rowIndex, "Name:", model.Name);
        rowIndex = AddTextRow(sheet, rowIndex, "Total posts:", model.PostCount.ToString());
        rowIndex = AddTextRow(sheet, rowIndex, "Verified posts:", model.VerifyPostCount.ToString());
        rowIndex = AddTextRow(sheet, rowIndex, "Unverified posts:", model.NotVerifyPostCount.ToString());

        rowIndex++;

        rowIndex = AddTable(sheet, rowIndex, "Game Statistics", new List<string> { "Game", "Post Count" }, model.GameStats, gs => new object[] { gs.Name, gs.PostsCount });

        rowIndex = AddTable(sheet, rowIndex, "Theme Statistics", new List<string> { "Theme", "Post Count" }, model.ThemesStats, ts => new object[] { ts.Name, ts.PostsCount });

        rowIndex = AddTable(sheet, rowIndex, "Post Summary", new List<string> { "ID", "Blog Name", "Title", "Likes", "Dislikes", "Comments", "Approved" },
            model.PostShortStats, ps => new object[] { ps.Id, ps.BlogName, ps.Name, ps.LikeCount, ps.NotLikeCount, ps.ComentsCount, ps.Verify });

        using var stream = new MemoryStream();
        workbook.Write(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{model.Name}.xlsx");
    }

    private int AddTextRow(ISheet sheet, int rowIndex, string label, string value)
    {
        var row = sheet.CreateRow(rowIndex);
        row.CreateCell(0).SetCellValue(label);
        row.CreateCell(1).SetCellValue(value);
        return rowIndex + 1;
    }

    private int AddTable<T>(ISheet sheet, int rowIndex, string title, List<string> headers, List<T> data, Func<T, object[]> selector)
    {
        if (data == null || data.Count == 0) return rowIndex;

        var titleRow = sheet.CreateRow(rowIndex++);
        titleRow.CreateCell(0).SetCellValue(title);

        var headerRow = sheet.CreateRow(rowIndex++);
        for (int i = 0; i < headers.Count; i++)
            headerRow.CreateCell(i).SetCellValue(headers[i]);

        foreach (var item in data)
        {
            var row = sheet.CreateRow(rowIndex++);
            var values = selector(item);
            for (int i = 0; i < values.Length; i++)
                row.CreateCell(i).SetCellValue(values[i]?.ToString());
        }

        return rowIndex + 1;
    }

    public async Task<IActionResult> DownloadPdf(StatsViewModel model)
    {
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content().Column(col =>
                {
                    col.Item().Text($"Report: {model.Name}").FontSize(20).Bold();
                    col.Item().Text($"Total posts: {model.PostCount}");
                    col.Item().Text($"Verified posts: {model.VerifyPostCount}");
                    col.Item().Text($"Unverified posts: {model.NotVerifyPostCount}");

                    if (model.GameStats?.Any() == true)
                    {
                        col.Item().Text("Game Statistics").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Game").Bold();
                                header.Cell().Text("Post Count").Bold();
                            });

                            foreach (var game in model.GameStats)
                            {
                                table.Cell().Text(game.Name);
                                table.Cell().Text(game.PostsCount.ToString());
                            }
                        });
                    }

                    if (model.ThemesStats?.Any() == true)
                    {
                        col.Item().Text("Theme Statistics").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Theme").Bold();
                                header.Cell().Text("Post Count").Bold();
                            });

                            foreach (var theme in model.ThemesStats)
                            {
                                table.Cell().Text(theme.Name);
                                table.Cell().Text(theme.PostsCount.ToString());
                            }
                        });
                    }

                    if (model.PostShortStats?.Any() == true)
                    {
                        col.Item().Text("Post Summary").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("ID").Bold();
                                header.Cell().Text("Blog").Bold();
                                header.Cell().Text("Title").Bold();
                                header.Cell().Text("Likes").Bold();
                                header.Cell().Text("Dislikes").Bold();
                                header.Cell().Text("Comments").Bold();
                                header.Cell().Text("Approved").Bold();
                            });

                            foreach (var post in model.PostShortStats)
                            {
                                table.Cell().Text(post.Id.ToString());
                                table.Cell().Text(post.BlogName);
                                table.Cell().Text(post.Name);
                                table.Cell().Text(post.LikeCount.ToString());
                                table.Cell().Text(post.DislikeCount.ToString());
                                table.Cell().Text(post.CommentCount.ToString());
                                table.Cell().Text(post.Verify.ToString());
                            }
                        });
                    }
                });
            });
        });

        using var stream = new MemoryStream();
        pdf.GeneratePdf(stream);
        return File(stream.ToArray(), "application/pdf", $"{model.Name}.pdf");
    }
}
