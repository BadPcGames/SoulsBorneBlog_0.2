using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1;
using WebApplication1.Models;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services;
using Newtonsoft.Json.Linq;
using WebApplication1.Controllers;
using Newtonsoft.Json;


namespace SysteamTest
{

    [TestFixture]
    public class SysteamStatsTest
    {
        private StatsController _statsController;
        private EmailService _emailService;
        private AppDbContext _dbContext;
        private IConfiguration _config;
        private UserService _userService;
        private IHttpContextAccessor _contextAccessor;


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
                    _config.GetConnectionString("DefaultConnection2"),
                    ServerVersion.AutoDetect(_config.GetConnectionString("DefaultConnection2"))
                )
                .Options;

            _contextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            _userService = new UserService(_dbContext, _contextAccessor);

            _dbContext = new AppDbContext(options);
            _emailService = new EmailService();
            _userService=new UserService(_dbContext, _contextAccessor);
            _statsController = new StatsController(_dbContext);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        //TS25-1
        [Test]
        public async Task TS25_1()
        {
            var result = await _statsController.GetUserStats("01.01.2024", "05.04.2025",12) as JsonResult;

            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Статистики немає", jsonData["message"].Value<string>());
        }
        //TS25-2
        [Test]
        public async Task TS25_2()
        {
            var result = await _statsController.GetUserStats("01.01.2024", "05.04.2025", 11) as JsonResult;

            var stats = result.Value as List<PostShortStatsViewModel>;
            Assert.IsNotNull(stats);

            Assert.AreEqual(3, stats.Count); ;
        }
        //TS26-1
        [Test]
        public async Task TS26_1()
        {
            var result = await _statsController.GetUserStats("01.01.2024", "05.04.2025", 26) as JsonResult;

            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Статистики немає", jsonData["message"].Value<string>());
        }
        //TS26-2
        [Test]
        public async Task TS26_2()
        {
            var result = await _statsController.GetPostStats(108) as JsonResult;

            var stats = result.Value as PostShortStatsViewModel;
            Assert.AreEqual(_dbContext.Posts.FirstOrDefault(post => post.Id == 108).Title, stats.Name);
        }
        //TS26-3
        [Test]
        public async Task TS26_3()
        {
            var result = await _statsController.GetUserStats("02.01.2025", "07.03.2025", 11) as JsonResult;

            var stats = result.Value as List<PostShortStatsViewModel>;
            Assert.IsNotNull(stats);

            Assert.AreEqual(3, stats.Count); ;
        }
        //TS26-4
        [Test]
        public async Task TS26_4()
        {
            var result = await _statsController.GetUserStats("02 березня 2025", "07.03.2025", 11) as JsonResult;

     
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Початковий час вказано не у вірному форматі", jsonData["message"].Value<string>());
        }
        //TS26-5
        [Test]
        public async Task TS26_5()
        {
            var result = await _statsController.GetUserStats("", "07.03.2025", 11) as JsonResult;

 
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Всі поля мають бути заповені", jsonData["message"].Value<string>());
        }
        //TS26-6
        [Test]
        public async Task TS26_6()
        {
            var result = await _statsController.GetUserStats("02.01.2025", "", 11) as JsonResult;


            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Всі поля мають бути заповені", jsonData["message"].Value<string>());
        }
        //TS26-7
        [Test]
        public async Task TS26_7()
        {
            var result = await _statsController.GetUserStats("02.01.2026", "07.03.2025", 11) as JsonResult;


            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Невірний часовий проміжок", jsonData["message"].Value<string>());
        }
        //TS27-1
        [Test]
        public async Task TS27_1()
        {
            var result = await _statsController.GetGlobalStats("02.01.2025", "07.03.2025") as JsonResult;

            var stats = result.Value as StatsViewModel;
            Assert.IsNotNull(stats);
            Assert.AreEqual(stats.PostCount, _dbContext.Posts.Count());
            
        }
        //TS27-2
        [Test]
        public async Task TS27_2()
        {
            var result = await _statsController.GetGlobalStats("02 березня 2025", "07.03.2025") as JsonResult;

            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Початковий час вказано не у вірному форматі", jsonData["message"].Value<string>());
        }
        //TS27-3
        [Test]
        public async Task TS27_3()
        {
            var result = await _statsController.GetGlobalStats("", "07.03.2025") as JsonResult;

            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Всі поля мають бути заповені", jsonData["message"].Value<string>());
        }
        //TS27-4
        [Test]
        public async Task TS27_4()
        {
            var result = await _statsController.GetGlobalStats("02.01.2025", "") as JsonResult;

            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Всі поля мають бути заповені", jsonData["message"].Value<string>());
        }
        //TS27-5
        [Test]
        public async Task TS27_5()
        {
            var result = await _statsController.GetGlobalStats("02.01.2026", "07.03.2025") as JsonResult;
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Невірний часовий проміжок", jsonData["message"].Value<string>());
        }
        //TS27-6
        [Test]
        public async Task TS27_6()
        {
            var result = await _statsController.GetFileStats("02.01.2025", "07.03.2025","ex") as FileContentResult;

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        }
        //TS27-7
        [Test]
        public async Task TS27_7()
        {
            var result = await _statsController.GetFileStats("02.01.2025", "07.03.2025", "pdf") as FileContentResult;

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("application/pdf", result.ContentType);
        }



        //TS29-3
        [Test]
        public async Task TS29_3()
        {
            var result = await _statsController.GetGlobalStats("02.01.2025", "07.03.2025") as JsonResult;

            var jsonObject = JObject.FromObject(result.Value);

            Assert.IsFalse(jsonObject.ContainsKey("password"));
        }

        //TS29-4
        [Test]
        public async Task TS29_4()
        {   
            Assert.AreEqual(_dbContext.Users.Where(user=>user.PasswordHash=="123456").Count(),0);
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _statsController?.Dispose();        
        }
    }
}
