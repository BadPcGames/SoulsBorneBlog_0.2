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

namespace SysteamTest
{
    [TestFixture]
    public class SysteamUserReactions1_1
    {
        private UserService _userService;
        private AppDbContext _dbContext;
        private IConfiguration _config;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private PostsController _postsController;
        private ModerController _moderController;
        private EmailService _emailService;

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
            new Claim(ClaimTypes.System, "26"), 
            new Claim(ClaimTypes.Name, "bad1"),
            new Claim(ClaimTypes.Email, "bad1@gmail.com"),
            new Claim(ClaimTypes.Role, "User") 
        }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(userClaims);

 
            _userService = new UserService(_dbContext, _httpContextAccessorMock.Object);
            _postsController = new PostsController(_dbContext, _userService, _config);
            _emailService = new EmailService();
            _moderController = new ModerController(_dbContext, _emailService);
        }



        [Test]
        public async Task TS10_04()
        {
            _postsController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.System, "26"),
                    new Claim(ClaimTypes.Name, "bad1"),
                    new Claim(ClaimTypes.Email, "bad1@gmail.com"),
                    new Claim(ClaimTypes.Role, "User")
                    }))
                }
            };

            await _moderController.TemporaryBan(26, "1");

            int likesCount = _dbContext.Reactions.Where(reaction => reaction.Value == -1).Count();
            await _postsController.MakeReactions(-1, 108);

            Assert.AreEqual(_dbContext.Reactions.Where(reaction => reaction.Value == -1).Count(), likesCount);
            await _moderController.DeleteBan(26);
        }


        [TearDown]
        public void TearDown()
        {
            _moderController?.Dispose();
            _postsController.Dispose();
            _dbContext.Dispose();
        }
    }

}
