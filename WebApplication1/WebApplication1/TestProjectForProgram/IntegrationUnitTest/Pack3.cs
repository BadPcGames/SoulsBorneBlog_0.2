using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WebApplication1;
using WebApplication1.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;


namespace IntegrationUnitTest
{
    [TestFixture]
    public class Pack3
    {
        private UserService _userService;
        private AppDbContext _dbContext;
        private IConfiguration _config;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private PostsController _postsController;
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
                   _config.GetConnectionString("DefaultConnection"),
                   ServerVersion.AutoDetect(_config.GetConnectionString("DefaultConnection"))
               )
               .Options;

            _dbContext = new AppDbContext(options);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.System, "13"), 
            new Claim(ClaimTypes.Name, "user1"),
            new Claim(ClaimTypes.Email, "user1@gmail.com"),
            new Claim(ClaimTypes.Role, "User") 
            }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(userClaims);

            _userService = new UserService(_dbContext, _httpContextAccessorMock.Object);
            _postsController = new PostsController(_dbContext, _userService, _config);

            _contextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
        }


        [Test]
        public async Task TS10_01()
        {
            // Имитация отсутствия аутентификации
            _contextAccessor.HttpContext.User = null;
            var postId = 0;

            // Вызов метода
            var result = await _postsController.MakeReactions(1, postId) as ObjectResult;

            // Проверяем, что возвращённый результат - ObjectResult со статусом 401
            Assert.NotNull(result);
            Assert.AreEqual(401, result.StatusCode);

            // Извлекаем redirectUrl из JSON-ответа
            var jsonString = JsonSerializer.Serialize(result.Value);
            var responseData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            Assert.NotNull(responseData);
            Assert.IsTrue(responseData.ContainsKey("redirectUrl"));
            Assert.AreEqual("/Auth/Index", responseData["redirectUrl"]);
        }
        [Test]
        public async Task TS10_02()
        {
            _postsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.System, "13"),
                new Claim(ClaimTypes.Name, "user1"),
                new Claim(ClaimTypes.Email, "user1@gmail.com"),
                new Claim(ClaimTypes.Role, "User")
            }))
                }
            };

            int likesCount = _dbContext.Reactions.Where(reaction => reaction.Value == 1).Count();
            await _postsController.MakeReactions(1, 0);

            Assert.AreEqual(_dbContext.Reactions.Where(reaction => reaction.Value == 1).Count(), likesCount + 1);
            await _postsController.MakeReactions(1, 0);
        }


        [Test]
        public async Task TS10_03()
        {
            _postsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.System, "13"),
                new Claim(ClaimTypes.Name, "user1"),
                new Claim(ClaimTypes.Email, "user1@gmail.com"),
                new Claim(ClaimTypes.Role, "User")
            }))
                }
            };

            int likesCount = _dbContext.Reactions.Where(reaction => reaction.Value == -1).Count();
            await _postsController.MakeReactions(-1, 0);

            Assert.AreEqual(_dbContext.Reactions.Where(reaction => reaction.Value == -1).Count(), likesCount + 1);
            await _postsController.MakeReactions(-1, 0);
        }


        [Test]
        public async Task TS10_05()
        {
            _postsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.System, "13"),
                new Claim(ClaimTypes.Name, "user1"),
                new Claim(ClaimTypes.Email, "user1@gmail.com"),
                new Claim(ClaimTypes.Role, "User")
            }))
                }
            };

            int comentCount = _dbContext.Coments.Where(com => com.PostId == 0).Count();
            await _postsController.MakeComent("гарно сказано", 0);

            Assert.AreEqual(_dbContext.Coments.Where(com => com.PostId == 0).Count(), comentCount + 1);
            int id = -1;
            foreach(var coment in _dbContext.Coments.Where(com => com.PostId == 0))
            {
                if(id<coment.Id)coment.Id = id;
            }
            await _postsController.DeleteComment(id);
        }


        [Test]
        public async Task TS10_06()
        {
            _postsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.System, "13"),
                new Claim(ClaimTypes.Name, "user1"),
                new Claim(ClaimTypes.Email, "user1@gmail.com"),
                new Claim(ClaimTypes.Role, "User")
            }))
                }
            };

            int comentCount = _dbContext.Coments.Where(com => com.PostId == 0).Count();
            await _postsController.MakeComent("", 0);

            Assert.AreEqual(_dbContext.Coments.Where(com => com.PostId == 0).Count(), comentCount);
       
        }

        [TearDown]
        public void TearDown()
        {
            _postsController.Dispose();
            _dbContext.Dispose();
        }
    }

}
