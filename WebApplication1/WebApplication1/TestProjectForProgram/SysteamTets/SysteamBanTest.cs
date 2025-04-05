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
    public class SysteamBanTest
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


        //TS19-2 Модератор переходить до списку публікацій, при відсутності нових публікацій, для перевірки з меню модератора 
        [Test, Order(1)]
        public async Task TS19_2()
        {
            var result = await _postsController.GetPostsToVerify() as JsonResult;
            Assert.IsNotNull(result, "Результат не повинен бути null");

            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>(), "success должен быть true");
            Assert.AreEqual("Публікацій не знайдено", jsonData["message"].Value<string>());
        }



        //TS19-1 Модератор переходить до списку публікацій для перевірки з меню модератора 
        [Test, Order(2)]
        public async Task TS19_1()
        {

            Post post = new Post()
            {
                Title = "DS1 was great",
                Game = "Dark Souls 1"
            };

            List<PostContentViewModel> contents = new List<PostContentViewModel>();
            PostContentViewModel content = new PostContentViewModel()
            {
                Position = 0,
                ContentType = "Text",
                Content = "Dark Sousl 1 was my first soulslike games, and I like it"
            };

            contents.Add(content);
            await _postsController.Create(17, post, contents);

            var result = await _postsController.GetPostsToVerify() as JsonResult;

            Assert.IsNotNull(result);

            var jsonData = JsonConvert.SerializeObject(result.Value);
            var postViewModels = JsonConvert.DeserializeObject<List<PostViewModel>>(jsonData);

            Assert.IsNotNull(postViewModels);
            Assert.IsInstanceOf<List<PostViewModel>>(postViewModels);

            Assert.IsTrue(postViewModels.Any());

            Assert.IsNotNull(postViewModels.First().Title);

            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > postId)
                {
                    postId = posts.Id;
                }
            }
        }


        //TS19-3 Модератор обирає публікацію для видалення, та вводить "Незадовільний текст" у поле "Reason"
        [Test, Order(3)]
        public async Task TS19_3()
        {
            string reason = "Незадовільний текст";
            int notApproval = 0;
            notApproval=_dbContext.Posts.Where(post=>post.Verify==false).Count();
            var result = await _postsController.PostNotApproval(postId, reason) as JsonResult;
            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Повідомлення відправлено", jsonData["message"].Value<string>());

            Assert.AreEqual(notApproval, _dbContext.Posts.Where(post => post.Verify == false).Count());
        }

        //TS19-4 Модератор обирає публікацію для видалення, та вводить "" у поле "Reason"
        [Test, Order(4)]
        public async Task TS19_4()
        {
            string reason = "";
            int notApproval = 0;
            notApproval = _dbContext.Posts.Where(post => post.Verify == false).Count();
            var result = await _postsController.PostNotApproval(postId, reason) as JsonResult;
            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Причина не написана", jsonData["message"].Value<string>());

            Assert.AreEqual(notApproval, _dbContext.Posts.Where(post => post.Verify == false).Count());
        }

        //TS19-5 Модератор обирає публікацію для видалення, та вводить "Тому" у поле "Reason"
        [Test, Order(5)]
        public async Task TS19_5()
        {
            string reason = "Тому";
            int notApproval = 0;
            notApproval = _dbContext.Posts.Where(post => post.Verify == false).Count();
            var result = await _postsController.PostNotApproval(postId, reason) as JsonResult;
            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Опиши причину детальніше", jsonData["message"].Value<string>());

            Assert.AreEqual(notApproval, _dbContext.Posts.Where(post => post.Verify == false).Count());

            await _postsController.Delete(postId);
        }

        //TS22-1 Модератор перейшов до списку користувачів, при відсутності доступних користувачів
        [Test,Order(6)]
        public async Task TS22_1()
        {
            var result =await _authController.GetUsers() as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Не знайдено доступних користувачів", jsonData["message"].Value<string>());
        }
        //TS24-1 Модератор перейшов до списку користувачів, при відсутності доступних користувачів
        [Test, Order(7)]
        public async Task TS24_1()
        {
            var result = await _authController.GetUsers() as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Не знайдено доступних користувачів", jsonData["message"].Value<string>());
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



        //TS24-2 Модератор перейшов до списку користувачів, натискає "Delete" та вводить "" у поле "Reason"
        [Test,Order(11)]
        public async Task TS24_2()
        {
            var user = new RegisterModel
            {
                Name = "superBadUser",
                Email = "superBadUser@gmail.com",
                Password = "123456"
            };

            await _authController.Register(user);

            var goalUser = _dbContext.Users.FirstOrDefault(user => user.Name == "superBadUser");
            userId=goalUser.Id;

            string reason = "";

            var result = await _moderController.AbsoluteBanUser(userId,reason) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Причина не вказана", jsonData["message"].Value<string>());
        }
        //TS24-3 Модератор перейшов до списку користувачів, натискає "Delete" та вводить "Тому" у поле "Reason"
        [Test, Order(12)]
        public async Task TS24_3()
        {
            string reason = "Тому";

            var result = await _moderController.AbsoluteBanUser(userId, reason) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Причина не має бути менше 15 симовлів", jsonData["message"].Value<string>());
        }
        //TS24-4 Модератор перейшов до списку користувачів, натискає "Delete" та вводить "Погана поведінка" у поле "Reason", за умови першого порушення
        [Test, Order(13)]
        public async Task TS24_4()
        {
            string reason = "Погана поведінка";

            var result = await _moderController.AbsoluteBanUser(userId, reason) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Користувачу надано попередження", jsonData["message"].Value<string>());
        }
        //TS24-5 Модератор перейшов до списку користувачів, натискає "Delete" та вводить "Погана поведінка" у поле "Reason", за умови багаторазового порушення
        [Test, Order(14)]
        public async Task TS24_5()
        {
            string reason = "Погана поведінка";

            var result = await _moderController.AbsoluteBanUser(userId, reason) as JsonResult;

            Assert.IsNotNull(result);
            var jsonData = JObject.FromObject(result.Value);
            Assert.IsTrue(jsonData["success"].Value<bool>());
            Assert.AreEqual("Куристувач був видалений з платворми!", jsonData["message"].Value<string>());

            Assert.AreEqual(0, _dbContext.Users.Where(user=>user.Name== "superBadUser").Count());
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
