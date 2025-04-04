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


    public async Task<StatsViewModel> MakeGlobalStats(DateTime from, DateTime to)
    {
        StatsViewModel stats = new StatsViewModel();
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
                ComentsCount = _context.Coments.Count(com => com.PostId == post.Id),
                BlogName=_context.Blogs.FirstOrDefault(blog=>blog.Id==post.BlogId).Name,
                Verify=post.Verify
            }).ToListAsync();

        return stats;
    }

  

    public async Task<PostShortStatsViewModel> MakePostStats(int postId)
    {
        PostShortStatsViewModel stats= new PostShortStatsViewModel();

        var post = _context.Posts.FirstOrDefault(post=>post.Id==postId);

        stats = new PostShortStatsViewModel()
        {
            Name = post.Title,
            Id = post.Id,
            LikeCount = _context.Reactions.Count(reaction => reaction.Value == 1 && reaction.PostId == post.Id),
            NotLikeCount = _context.Reactions.Count(reaction => reaction.Value == -1 && reaction.PostId == post.Id),
            ComentsCount = _context.Coments.Count(com => com.PostId == post.Id),
            BlogName=_context.Blogs.FirstOrDefault(blog=>blog.Id==post.BlogId).Name,
            Verify = post.Verify
        };
        return stats;
    }

    public async Task<List<PostShortStatsViewModel>> MakeUserStats(DateTime from, DateTime to, int userId)
    {
        List<PostShortStatsViewModel> stats = new List<PostShortStatsViewModel>();

        var blogs = _context.Blogs
                    .Where(blog => blog.AuthorId == userId)
                    .Select(blog => blog.Id);

        var posts = _context.Posts.Where(post => post.CreatedAt >= from && post.CreatedAt <= to && blogs.Contains(post.BlogId));

        stats = await posts
            .Select(post => new PostShortStatsViewModel
            {
                Name = post.Title,
                Id = post.Id,
                LikeCount = _context.Reactions.Count(reaction => reaction.Value == 1 && reaction.PostId == post.Id),
                NotLikeCount = _context.Reactions.Count(reaction => reaction.Value == -1 && reaction.PostId == post.Id),
                ComentsCount = _context.Coments.Count(com => com.PostId == post.Id),
                BlogName = _context.Blogs.FirstOrDefault(blog => blog.Id == post.BlogId).Name,
                Verify = post.Verify
            }).ToListAsync();

        return stats;
    }


    public async Task<IActionResult> GetUserStats(string? TimeStart, string? TimeEnd, int userId)
    {
        if (TimeStart == "" || TimeStart == null || TimeEnd == "" || TimeEnd == null)
        {
            return Json(new { success = true, message = "�� ���� ����� ���� ��������" });
        }

        DateTime from, to;

        if (!DateTime.TryParse(TimeStart, out from))
        {
            return Json(new { success = true, message = "���������� ��� ������� �� � ������ ������" });
        }

        if (!DateTime.TryParse(TimeEnd, out to))
        {
            return Json(new { success = true, message = "���������� ��� ������� �� � ������ ������" });
        }

        if (from > to || from > DateTime.Now || to > DateTime.Now)
        {
            return Json(new { success = true, message = "������� ������� �������" });
        }
        var stats = await MakeUserStats(from, to,userId);

        if (stats.Count() == 0)
        {
            return Json(new { success = true, message = "���������� ����" });
        }
        return Json(stats);
    }

    public async Task<IActionResult> GetPostStats(int postId)
    {
        var stats = await MakePostStats( postId);
        return Json(stats);
    }

    public async Task<IActionResult> GetGlobalStats(string? TimeStart, string? TimeEnd)
    {
        if (TimeStart == "" || TimeStart == null || TimeEnd == "" || TimeEnd == null)
        {
            return Json(new { success = true, message = "�� ���� ����� ���� ��������" });
        }

        DateTime from, to;

        if (!DateTime.TryParse(TimeStart, out from))
        {
            return Json(new { success = true, message = "���������� ��� ������� �� � ������ ������" });
        }

        if (!DateTime.TryParse(TimeEnd, out to))
        {
            return Json(new { success = true, message = "���������� ��� ������� �� � ������ ������" });
        }

        if (from > to || from > DateTime.Now || to > DateTime.Now)
        {
            return Json(new { success = true, message = "������� ������� �������" });
        }
        var stats = await MakeGlobalStats(from, to);
        return Json(stats);
    }

    public async Task<IActionResult> GetFileStats(string? TimeStart, string? TimeEnd,string type)
    {
        if (string.IsNullOrEmpty(TimeStart) || string.IsNullOrEmpty(TimeEnd))
        {
            return Json(new { success = false, message = "�� ���� ����� ���� ��������" });
        }

        DateTime from, to;

        if (!DateTime.TryParse(TimeStart, out from))
        {
            return Json(new { success = false, message = "���������� ��� ������� �� � ������ ������" });
        }

        if (!DateTime.TryParse(TimeEnd, out to))
        {
            return Json(new { success = false, message = "���������� ��� ������� �� � ������ ������" });
        }

        if (from > to || from > DateTime.Now || to > DateTime.Now)
        {
            return Json(new { success = false, message = "������� ������� �������" });
        }

        var stats = await MakeGlobalStats(from, to);
        if (type == "ex")
        {
            var fileResult = await DownloadExcel(stats);
            return fileResult;
        }
        else
        {
            var fileResult = await DownloadPdf(stats);
            return fileResult;
        }
        
    }

    public async Task<IActionResult> DownloadExcel(StatsViewModel model)
    {
        using var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("���");

        int rowIndex = 0;

        rowIndex = AddTextRow(sheet, rowIndex, "�����:", model.Name);
        rowIndex = AddTextRow(sheet, rowIndex, "������ �����:", model.PostCount.ToString());
        rowIndex = AddTextRow(sheet, rowIndex, "ϳ���������� �����:", model.VerifyPostCount.ToString());
        rowIndex = AddTextRow(sheet, rowIndex, "�������������� �����:", model.NotVerifyPostCount.ToString());

        rowIndex++;

        rowIndex = AddTable(sheet, rowIndex, "���������� �� �����", new List<string> { "���", "ʳ������ �����" },
            model.GameStats, gs => new object[] { gs.Name, gs.PostsCount });

        rowIndex = AddTable(sheet, rowIndex, "���������� ���", new List<string> { "����", "ʳ������ �����" },
            model.ThemesStats, ts => new object[] { ts.Name, ts.PostsCount });

        rowIndex = AddTable(sheet, rowIndex, "������� ���������� �����",
            new List<string> { "ID","����� �����", "�����", "�����", "ĳ������", "��������","���������" },
            model.PostShortStats, ps => new object[] { ps.Id, ps.BlogName,ps.Name, ps.LikeCount, ps.NotLikeCount, ps.ComentsCount,ps.Verify });

        using var stream = new MemoryStream();
        workbook.Write(stream);
        var content = stream.ToArray();

        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{model.Name}.xlsx");
    }

    private int AddTextRow(ISheet sheet, int rowIndex, string label, string value)
    {
        var row = sheet.CreateRow(rowIndex);
        row.CreateCell(0).SetCellValue(label);
        row.CreateCell(1).SetCellValue(value);
        return rowIndex + 1;
    }

    private int AddTable<T>(ISheet sheet, int rowIndex, string title, List<string> headers, List<T> data, System.Func<T, object[]> selector)
    {
        if (data == null || data.Count == 0)
            return rowIndex;

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

        rowIndex++;
        return rowIndex;
    }

    public async Task<IActionResult> DownloadPdf(StatsViewModel model)
    {
        var pdfDocument = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content().Column(col =>
                {
                    col.Item().Text($"���: {model.Name}").FontSize(20).Bold();
                    col.Item().Text($"������ �����: {model.PostCount}");
                    col.Item().Text($"ϳ���������� �����: {model.VerifyPostCount}");
                    col.Item().Text($"������������� �����: {model.NotVerifyPostCount}");

                    if (model.GameStats?.Any() == true)
                    {
                        col.Item().Text("���������� �� �����").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("���").Bold();
                                header.Cell().Text("ʳ������ �����").Bold();
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
                        col.Item().Text("���������� ���").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("����").Bold();
                                header.Cell().Text("ʳ������ �����").Bold();
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
                        col.Item().Text("������� ���������� �����").FontSize(16).Bold();
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
                                header.Cell().Text("����� �����").Bold();
                                header.Cell().Text("�����").Bold();
                                header.Cell().Text("�����").Bold();
                                header.Cell().Text("ĳ������").Bold();
                                header.Cell().Text("��������").Bold();
                                header.Cell().Text("���������").Bold();
                            });

                            foreach (var post in model.PostShortStats)
                            {
                                table.Cell().Text(post.Id.ToString());
                                table.Cell().Text(post.BlogName);
                                table.Cell().Text(post.Name);
                                table.Cell().Text(post.LikeCount.ToString());
                                table.Cell().Text(post.NotLikeCount.ToString());
                                table.Cell().Text(post.ComentsCount.ToString());
                                table.Cell().Text(post.Verify.ToString());
                            }
                        });
                    }
                });
            });
        });

        using var stream = new MemoryStream();
        pdfDocument.GeneratePdf(stream);
        return File(stream.ToArray(), "application/pdf", $"{model.Name}.pdf");
    }

}
