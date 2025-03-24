using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1;
using WebApplication1.Controllers;
using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;

namespace AuthUnitTest
{
    [TestFixture]
    public class GameUnitTest
    {
        private GameController _gameController;
        private AppDbContext _dbContext;
        private IConfiguration _config;

        int gameIndex = 0;

        [SetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)  
                .AddJsonFile("TestAppSettings.json", optional: false, reloadOnChange: true)
                .Build();

            _config = configuration;

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(
                    _config.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(_config.GetConnectionString("DefaultConnection"))
                )
                .Options;

            _dbContext = new AppDbContext(options);
            _gameController = new GameController(_dbContext);
        }

        public IFormFile CreateIFormFileFromPath(string filePath)
        {
            var stream = new MemoryStream(File.ReadAllBytes(filePath));
            return new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(filePath));
        }

        //TS11-1 +
        [Test,Order(1)]
        public async Task TS11_1()
        {

            IFormFile file = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png");
            var result = await _gameController.Create("DS", "Not Dark Souls", file, "#fffff") as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        //TS11-2 -
        [Test, Order(2)]
        public async Task TS11_2()
        {

            IFormFile file = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png");
            var result = await _gameController.Create("DS","",file,"#fffff") as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Create", result.ActionName);
            Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        }

        //TS13-1 -
        [Test, Order(3)]
        public async Task TS13_1()
        {
            foreach(var games in _dbContext.Games)
            {
                if (games.Id > gameIndex)
                {
                    gameIndex = games.Id;
                }
            }

            IFormFile file = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png");
            var result = await _gameController.Edit(gameIndex,"DS", "Not Dark Souls", file, "#fffff") as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }
        //TS13-2 -
        [Test, Order(4)]
        public async Task TS13_2()
        {
            foreach (var games in _dbContext.Games)
            {
                if (games.Id > gameIndex)
                {
                    gameIndex = games.Id;
                }
            }

            IFormFile file = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png");
            var result = await _gameController.Edit(gameIndex,"DS", "", file, "#fffff") as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Edit", result.ActionName);
            Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        }

        //TS12 +
        [Test, Order(5)]
        public async Task TS12()
        {
            foreach (var games in _dbContext.Games)
            {
                if (games.Id > gameIndex)
                {
                    gameIndex = games.Id;
                }
            }

            var result = await _gameController.Delete(gameIndex) as RedirectToActionResult;
            Console.WriteLine(gameIndex);
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }



        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _gameController?.Dispose();
        }
    }
}
