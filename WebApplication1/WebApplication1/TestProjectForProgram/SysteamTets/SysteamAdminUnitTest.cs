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
using WebApplication1.Services;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

namespace SysteamTest
{
    [TestFixture]
    public class SysteamAdminUnitTest
    {
        private AdminController _adminController;
        private AuthController _authController;
        private AppDbContext _dbContext;
        private IConfiguration _config;
        private UserService _userService;
        private IHttpContextAccessor _contextAccessor;
        private EmailService _emailService;
        private IOptions<AdminEmailOptions> _adminEmail;

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

            _contextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            _dbContext = new AppDbContext(options);
            _userService = new UserService(_dbContext, _contextAccessor);
            _adminController = new AdminController(_dbContext);
            _emailService = new EmailService();
            var adminEmailOptions = _config.GetSection("AdminEmail").Get<AdminEmailOptions>();
            _adminEmail = Options.Create(adminEmailOptions);
            _authController = new AuthController(_dbContext, _config, _userService, _emailService, _adminEmail);

            _adminController.DeleteModer("user1", "user1@gmail.com");
            _adminController.DeleteModer("bad1", "bad1@gmail.com");
        }


        //TS21-1
        [Test, Order(1)]
        public async Task TS21_1()
        {
            var result = await _authController.GetModers() as JsonResult;

            var jsonObject = JObject.FromObject(result.Value);

            Assert.IsTrue((bool)jsonObject["success"]);
            Assert.AreEqual("Не знайдено доступних модераторів", (string)jsonObject["message"]);

        }
        //TS20-1
        [Test,Order(2)]
        public async Task TS20_1()
        {
            await _adminController.AddModer("user1", "user1@gmail.com");

            Assert.IsTrue(_dbContext.Users.FirstOrDefault(user => user.Name == "user1").Role == "Moder");
        }
        //TS20-2
        [Test, Order(3)]
        public async Task TS20_2()
        {
            var result=await _adminController.AddModer("user2", "user1@gmail.com") as JsonResult;

            var jsonObject = JObject.FromObject(result.Value);

            Assert.IsTrue((bool)jsonObject["success"]);
            Assert.AreEqual("Користувача з такими логіном та поштою не існує", (string)jsonObject["message"]);
        }
        //TS20-3
        [Test, Order(3)]
        public async Task TS20_3()
        {
            var result = await _adminController.AddModer("user1", "user1") as JsonResult;

            var jsonObject = JObject.FromObject(result.Value);

            Assert.IsTrue((bool)jsonObject["success"]);
            Assert.AreEqual("Користувача з такими логіном та поштою не існує", (string)jsonObject["message"]);
        }
        //TS20-4
        [Test, Order(4)]
        public async Task TS20_4()
        {
            var result = await _adminController.AddModer("user1", "") as JsonResult;

            var jsonObject = JObject.FromObject(result.Value);

            Assert.IsTrue((bool)jsonObject["success"]);
            Assert.AreEqual("Всі поля мають бути заповнені", (string)jsonObject["message"]);
        }
        //TS20-5
        [Test, Order(5)]
        public async Task TS20_5()
        {
            var result = await _adminController.AddModer("", "user1@gmail.com") as JsonResult;

            var jsonObject = JObject.FromObject(result.Value);

            Assert.IsTrue((bool)jsonObject["success"]);
            Assert.AreEqual("Всі поля мають бути заповнені", (string)jsonObject["message"]);
        }
        //TS21-2
        [Test, Order(6)]
        public async Task TS21_2()
        {
            await _adminController.DeleteModer("user1", "user1@gmail.com");

            Assert.IsTrue(_dbContext.Users.FirstOrDefault(user => user.Name == "user1").Role == "User");
            await _adminController.AddModer("bad1", "bad1@gmail.com");
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _adminController?.Dispose();
            _authController?.Dispose();
           
        }
    }
}
