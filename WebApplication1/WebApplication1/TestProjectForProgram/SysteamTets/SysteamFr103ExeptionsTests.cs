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
using WebApplication1.DbModels;

namespace SysteamTest
{
    [TestFixture]
    public class SysteamFr103ExeptionsTests
    {
        private AuthController _authController;
        private HomeController _homeController;
        private PostsController _postsController;
        private BlogsController _blogController;
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
                    _config.GetConnectionString("DefaultConnection5000"),
                    ServerVersion.AutoDetect(_config.GetConnectionString("DefaultConnection5000"))
                )
                .Options;

                _contextAccessor = new HttpContextAccessor
                {
                    HttpContext = new DefaultHttpContext()
                };

         
            _dbContext = new AppDbContext(options);

            _userService = new UserService(_dbContext, _contextAccessor);
            _authController = new AuthController(_dbContext, _config, _userService);
            _homeController = new HomeController(_dbContext);
            _postsController=new PostsController(_dbContext,_userService,_config);
            _blogController = new BlogsController(_dbContext, _userService);

            
        }


        public async Task LogIn()
        {
            var httpContextMock = new Mock<HttpContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var urlHelperMock = new Mock<IUrlHelper>();
            var authServiceMock = new Mock<IAuthenticationService>();

            serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService)))
                               .Returns(authServiceMock.Object);

            serviceProviderMock.Setup(x => x.GetService(typeof(IUrlHelperFactory)))
                               .Returns(new Mock<IUrlHelperFactory>().Object);
            httpContextMock.Setup(x => x.RequestServices)
                           .Returns(serviceProviderMock.Object);


            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            _authController.Url = urlHelperMock.Object;

            authServiceMock.Setup(x => x.SignInAsync(It.IsAny<HttpContext>(),
                                                     It.IsAny<string>(),
                                                     It.IsAny<ClaimsPrincipal>(),
                                                     It.IsAny<AuthenticationProperties>()))
                           .Returns(Task.CompletedTask);

            var user = new LoginModel
            {
                Email = "user1@gmail.com",
                Password = "123456"
            };

            var result = await _authController.Login(user);
        }


   

        [Test,Order(1)]
        //TS15-2
        public async Task TS17_2()
        {
            var user = new RegisterModel()
            {
                Name = "nf" + 50001,
                Email = "nf" + 50001 + "@gmail.com",
                Password = "123456"
            };
            var result = await _authController.Register(user) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("Too much users", result.RouteValues["message"]);
        }

        //TS16-2
        [Test, Order(2)]
        public async Task TS15_2()
        {
            await LogIn();
            Blog blog = new Blog()
            {
                Name = "DS blog",
                Description = "My first blog about Dark Souls",
                Theme = "Theories"
            };
            var result =await _blogController.Create(blog, 12) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }


        //TS17-2
        [Test,Order(3)]
        public async Task TS16_2()
        {  
            var result =  _postsController.Create(18,null) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }



        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _authController?.Dispose();
            _homeController?.Dispose();
            _postsController?.Dispose();
            _blogController?.Dispose();
        }
    }
}
