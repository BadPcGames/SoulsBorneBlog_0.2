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




namespace ValidationTest
{

    [TestFixture]
    public class ValidationBlogUnitTest
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

        ////TS07-1 +
        //[Test, Order(1)]
        //public async Task TS07_1()
        //{
        //    Post post = new Post()
        //    {
        //        Title = "DS1 was great",
        //        Game = "Dark Souls 1"
        //    };

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Text",
        //        Content = "Dark Sousl 1 was my first soulslike games, and I like it"
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    int toDelete = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toDelete)
        //        {
        //            toDelete = posts.Id;
        //        }
        //    }
        //    await _postsController.Delete(toDelete);
        //}
        ////TS07-2 +
        //[Test, Order(2)]
        //public async Task TS07_2()
        //{
        //    Post post = new Post()
        //    {
        //        Title = "DS1 was great",
        //        Game = "Dark Souls 1"
        //    };

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Image",
        //        FormFile = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png")
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    int toDelete = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toDelete)
        //        {
        //            toDelete = posts.Id;
        //        }
        //    }
        //    await _postsController.Delete(toDelete);
        //}
        ////TS07-3 +
        //[Test, Order(3)]
        //public async Task TS07_3()
        //{
        //    Post post = new Post()
        //    {
        //        Title = "DS1 was great",
        //        Game = "Dark Souls 1"
        //    };

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Video",
        //        FormFile = CreateIFormFileFromPath("D:\\Смотреть\\записи\\Captures\\video.mp4")
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //}
        ////TS07-4 -
        //[Test, Order(4)]
        //public async Task TS07_4()
        //{
        //    Post post = new Post()
        //    {
        //        Title = "",
        //        Game = "Dark Souls 1"
        //    };

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Text",
        //        Content = ""
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Create", result.ActionName);
        //    Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        //}

        ////TS09-1 +
        //[Test, Order(5)]
        //public async Task TS09_1()
        //{
        //    int toChange = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toChange)
        //        {
        //            toChange = posts.Id;
        //        }
        //    }

        //    Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
        //    post.Title = "DS1 was great";
        //    post.Game = "Dark Souls 1";

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Text",
        //        Content = "Dark Sousl 1 was my first soulslike games, and I like it"
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //}
        ////TS09-2 +
        //[Test, Order(6)]
        //public async Task TS09_2()
        //{
        //    int toChange = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toChange)
        //        {
        //            toChange = posts.Id;
        //        }
        //    }
        //    Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
        //    post.Title = "DS1 was great";
        //    post.Game = "Dark Souls 1";

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Image",
        //        FormFile = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png")
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);
        //}
        ////TS09-3 +
        //[Test, Order(7)]
        //public async Task TS09_3()
        //{
        //    int toChange = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toChange)
        //        {
        //            toChange = posts.Id;
        //        }
        //    }

        //    Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
        //    post.Title = "DS1 was great";
        //    post.Game = "Dark Souls 1";

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Video",
        //        FormFile = CreateIFormFileFromPath("D:\\Смотреть\\записи\\Captures\\video.mp4")
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //}
        ////TS07-9 -
        //[Test, Order(8)]
        //public async Task TS09_4()
        //{
        //    int toChange = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toChange)
        //        {
        //            toChange = posts.Id;
        //        }
        //    }

        //    Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
        //    post.Title = "";
        //    post.Game = "Dark Souls 1";

        //    List<PostContentViewModel> contents = new List<PostContentViewModel>();
        //    PostContentViewModel content = new PostContentViewModel()
        //    {
        //        Position = 0,
        //        ContentType = "Text",
        //        Content = ""
        //    };

        //    contents.Add(content);
        //    var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Edit", result.ActionName);
        //    Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        //}

        ////TS08
        //[Test, Order(9)]
        //public async Task TS08()
        //{
        //    int toDelete = 0;
        //    foreach (var posts in _dbContext.Posts)
        //    {
        //        if (posts.Id > toDelete)
        //        {
        //            toDelete = posts.Id;
        //        }
        //    }
        //    var result = await _postsController.Delete(toDelete) as RedirectToActionResult;
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);
        //}


        //[Test,Order(10)]
        //public async Task TS01()
        //{
        //    var result = await _postsController.GetPosts(1, 2, null, null, null);

        //    Assert.IsInstanceOf<JsonResult>(result);

        //    var jsonResult = (JsonResult)result;
        //    var posts = (List<PostViewModel>)jsonResult.Value;

        //    Assert.That(posts.Count, Is.EqualTo(_dbContext.Posts.Count() >= 2 ? 2 : _dbContext.Posts.Count()));

        //}


        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _blogController?.Dispose();
            _authController?.Dispose(); 
        }
    }
}
