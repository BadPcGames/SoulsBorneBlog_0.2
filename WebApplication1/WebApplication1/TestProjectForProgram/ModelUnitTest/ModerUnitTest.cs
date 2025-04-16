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


namespace ModelTest
{

    [TestFixture]
    public class ModerUnitTest
    {
        int postId = 0;
        int userId = 0;
        private PostsController _postsController;
        private ModerController _moderController;
        private EmailService _emailService;
        private AppDbContext _dbContext;
        private IConfiguration _config;
        private UserService _userService;
        private IHttpContextAccessor _contextAccessor;
        private AuthController _authController;

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
            _postsController = new PostsController(_dbContext, _userService, _config);
            _moderController = new ModerController(_dbContext, _emailService);
            _userService=new UserService(_dbContext, _contextAccessor);
            _authController = new AuthController(_dbContext, _config, _userService);
        }




        //TS23-1 Модератор перейшов до списку користувачів, при відсутності користувачів з обмеженнями
        [Test, Order(8)]
        public async Task TS23_1()
        {
            var result = await _authController.GetUsersForBunDelete() as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Користувачів з обмеженями немає", jsonData["message"].Value<string>());
        }

        //TS22-2 Модератор перейшов до списку користувачів, вписує "1" у поле "Time" та натискає "Send"
        [Test, Order(9)]
        public async Task TS22_2()
        {
            string time = "1";
            int id = 26;

            var result = await _moderController.TemporaryBan(id,time) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Користувач успішно забанин!", jsonData["message"].Value<string>());

            Assert.IsTrue(_dbContext.Users.FirstOrDefault(user=>user.Id==id).BanTime!=null);
        }
        //TS22-3 Модератор перейшов до списку користувачів, вписує "0" у поле "Time" та натискає "Send"
        [Test, Order(9)]
        public async Task TS22_3()
        {
            string time = "0";
            int id = 26;

            var result = await _moderController.TemporaryBan(id, time) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Час не вказано", jsonData["message"].Value<string>());
        }
        //TS22-4 Модератор перейшов до списку користувачів, вписує "" у поле "Time" та натискає "Send"
        [Test, Order(9)]
        public async Task TS22_4()
        {
            string time = "";
            int id = 26;

            var result = await _moderController.TemporaryBan(id, time) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Час не вказано", jsonData["message"].Value<string>());
        }
        //TS22-5 Модератор перейшов до списку користувачів, вписує "Годину" у поле "Time" та натискає "Send"
        [Test, Order(9)]
        public async Task TS22_5()
        {
            string time = "Годину";
            int id = 26;

            var result = await _moderController.TemporaryBan(id, time) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Час вказано не вірно", jsonData["message"].Value<string>());
        }

        //TS23-2 Модератор перейшов до списку користувачів, та натискає кнопку "Delete Ban" напроти обраного користувача з обмеженями
        [Test,Order(10)]
        public async Task TS23_2()
        {
            int id = 26;

            var result= await _moderController.DeleteBan(id) as JsonResult;

            Assert.IsTrue(_dbContext.Users.FirstOrDefault(user => user.Id == id).BanTime == null);
        }



       

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _postsController?.Dispose();
            _moderController?.Dispose();
            _authController?.Dispose();
           
        }
    }
}
