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
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Formula.Functions;
using Microsoft.JSInterop;
using QuestPDF.Helpers;
using QuestPDF.Fluent;


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
                ComentsCount = _context.Coments.Count(com => com.PostId == post.Id),
                Verify=post.Verify
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

    public async Task<IActionResult> GetFileStats(string? TimeStart, string? TimeEnd,string type)
    {
        if (string.IsNullOrEmpty(TimeStart) || string.IsNullOrEmpty(TimeEnd))
        {
            return Json(new { success = false, message = "Всі поля мають бути заповені" });
        }

        DateTime from, to;

        if (!DateTime.TryParse(TimeStart, out from))
        {
            return Json(new { success = false, message = "Початковий час вказано не у вірному форматі" });
        }

        if (!DateTime.TryParse(TimeEnd, out to))
        {
            return Json(new { success = false, message = "Остаточний час вказано не у вірному форматі" });
        }

        if (from > to || from > DateTime.Now || to > DateTime.Now)
        {
            return Json(new { success = false, message = "Невірний часовий проміжок" });
        }

        var stats = await MakeStats(from, to);
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

    public async Task<IActionResult> DownloadExcel(GlobalStatsViewModel model)
    {
        using var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Звіт");

        int rowIndex = 0;

        rowIndex = AddTextRow(sheet, rowIndex, "Назва:", model.Name);
        rowIndex = AddTextRow(sheet, rowIndex, "Всього постів:", model.PostCount.ToString());
        rowIndex = AddTextRow(sheet, rowIndex, "Підтверждені пости:", model.VerifyPostCount.ToString());
        rowIndex = AddTextRow(sheet, rowIndex, "Непідтвержедені пости:", model.NotVerifyPostCount.ToString());

        rowIndex++;

        rowIndex = AddTable(sheet, rowIndex, "Статистика по іграм", new List<string> { "Гра", "Кількість постів" },
            model.GameStats, gs => new object[] { gs.Name, gs.PostsCount });

        rowIndex = AddTable(sheet, rowIndex, "Статистика тем", new List<string> { "Тема", "Кількість постів" },
            model.ThemesStats, ts => new object[] { ts.Name, ts.PostsCount });

        rowIndex = AddTable(sheet, rowIndex, "Коротка інформація постів",
            new List<string> { "ID", "Назва", "Лайки", "Дізлайки", "Коментарі","Одобрення" },
            model.PostShortStats, ps => new object[] { ps.Id, ps.Name, ps.LikeCount, ps.NotLikeCount, ps.ComentsCount,ps.Verify });

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

    private int AddTable<T>(ISheet sheet, int rowIndex, string title, List<string> headers,
                            List<T> data, System.Func<T, object[]> selector)
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



    public async Task<IActionResult> DownloadPdf(GlobalStatsViewModel model)
    {
        var pdfDocument = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content().Column(col =>
                {
                    col.Item().Text($"Звіт: {model.Name}").FontSize(20).Bold();
                    col.Item().Text($"Всього постів: {model.PostCount}");
                    col.Item().Text($"Підтверждені пости: {model.VerifyPostCount}");
                    col.Item().Text($"Непідтверждені пости: {model.NotVerifyPostCount}");

                    if (model.GameStats?.Any() == true)
                    {
                        col.Item().Text("Статистика по іграм").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Гра").Bold();
                                header.Cell().Text("Кількість постів").Bold();
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
                        col.Item().Text("Статистика тем").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Тема").Bold();
                                header.Cell().Text("Кількість постів").Bold();
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
                        col.Item().Text("Коротка статистика постів").FontSize(16).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(50);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("ID").Bold();
                                header.Cell().Text("Назва").Bold();
                                header.Cell().Text("Лайки").Bold();
                                header.Cell().Text("Дізлайки").Bold();
                                header.Cell().Text("Коментарі").Bold();
                                header.Cell().Text("Одобрення").Bold();
                            });

                            foreach (var post in model.PostShortStats)
                            {
                                table.Cell().Text(post.Id.ToString());
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
