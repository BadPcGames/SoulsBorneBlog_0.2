using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1;
using WebApplication1.Models;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using WebApplication1.Controllers;
using System.Security.Claims;




namespace ModelTest
{ 
    [TestFixture]
    public class BlogUnitTest
    {
        private BlogsController _blogController;
        private AuthController _authController;
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
                    _config.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(_config.GetConnectionString("DefaultConnection"))
                )
                .Options;

            _contextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            _userService = new UserService(_dbContext, _contextAccessor);
            _dbContext = new AppDbContext(options);
            _blogController = new BlogsController(_dbContext,_userService);
            _authController = new AuthController(_dbContext, _config, _userService);
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

        //TS04-1
        [Test,Order(1)]
        public async Task TS04_1()
        {
            await LogIn();
            Blog blog = new Blog()
            {
                Name= "DS blog",
                Description= "My first blog about Dark Souls",
                Theme= "Theories"
            };

            int? before = _dbContext.Blogs.Count();
            await _blogController.Create(blog,13);
            int? after = _dbContext.Blogs.Count();
            Assert.AreEqual(before+1, after);
        }

   

        //TS04-2
        [Test, Order(2)]
        public async Task TS04_2()
        {
            Blog blog = new Blog()
            {
                Name = "",
                Description = "My first blog about Dark Souls",
                Theme = "Theories"
            };
            var result = await _blogController.Create(blog, 13);

            Assert.IsInstanceOf<ViewResult>(result); 
            var viewResult = result as ViewResult;
            Assert.AreEqual(blog, viewResult.Model); 
        }
        //TS05-1
        [Test, Order(3)]
        public async Task TS05_1()
        {
            var blogs = _dbContext.Blogs;
            int maxId = 0;

            foreach (var item in blogs)
            {
                if (item.Id > maxId)
                {
                    maxId = item.Id;
                }
            }

            Blog blog = new Blog()
            {
                Id = maxId,
                Name = "",
                Description = "My first blog about Dark Souls",
                Theme = "Theories"
            };


            var result = await _blogController.Edit(maxId, blog);

            Assert.IsInstanceOf<ViewResult>(result);
        }
        //TS05-2
        [Test, Order(4)]
        public async Task TS05_2()
        {
            var blogs = _dbContext.Blogs;
            int maxId = 0;

            foreach (var item in blogs)
            {
                if (item.Id > maxId)
                {
                    maxId = item.Id;
                }
            }

            Blog blog = new Blog()
            {
                Id=maxId,
                Name = "",
                Description = "My first blog about Dark Souls",
                Theme = "Theories"
            };

          
            var result = await _blogController.Edit(maxId, blog);

            Assert.IsInstanceOf<ViewResult>(result);
            var view = result as ViewResult;
            Assert.AreEqual(blog, view.Model);
        }
        //TS06
        [Test, Order(5)]
        public async Task TS06()
        {
            int? before = _dbContext.Blogs.Count();
            var blogs = _dbContext.Blogs;
            int maxId = 0;

            foreach (var item in blogs)
            {
                if (item.Id > maxId)
                {
                    maxId = item.Id;
                }
            }
            await _blogController.Delete(maxId);

            int? after = _dbContext.Blogs.Count();
            Assert.AreEqual(before, after + 1);

        }

  
        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _blogController?.Dispose();
            _authController?.Dispose(); 
        }
    }
}
