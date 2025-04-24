using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1;
using WebApplication1.Models;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;




namespace SysteamTest
{

    [TestFixture]
    public class SysteamPostUnitTest
    {

        public IFormFile CreateIFormFileFromPath(string filePath)
        {
            var stream = new MemoryStream(File.ReadAllBytes(filePath));
            return new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(filePath));
        }

        private PostsController _postsController;
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
            _postsController = new PostsController(_dbContext, _userService, _config);
        }


        //TS07-1 +
        [Test, Order(1)]
        public async Task TS07_1()
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
            var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            int toDelete = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toDelete)
                {
                    toDelete = posts.Id;
                }
            }
            await _postsController.Delete(toDelete);
        }

        [Test, Order(1)]
        public async Task TS16_1()
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
            var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            int toDelete = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toDelete)
                {
                    toDelete = posts.Id;
                }
            }
            await _postsController.Delete(toDelete);
        }

        //TS07-2 +
        [Test, Order(2)]
        public async Task TS07_2()
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
                ContentType = "Image",
                FormFile = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png")
            };

            contents.Add(content);
            var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            int toDelete = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toDelete)
                {
                    toDelete = posts.Id;
                }
            }
            await _postsController.Delete(toDelete);
        }
        //TS07-3 +
        [Test, Order(3)]
        public async Task TS07_3()
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
                ContentType = "Video",
                FormFile = CreateIFormFileFromPath("D:\\Смотреть\\записи\\Captures\\video.mp4")
            };

            contents.Add(content);
            var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

        }
        //TS07-4 -
        [Test, Order(4)]
        public async Task TS07_4()
        {
            Post post = new Post()
            {
                Title = "",
                Game = "Dark Souls 1"
            };

            List<PostContentViewModel> contents = new List<PostContentViewModel>();
            PostContentViewModel content = new PostContentViewModel()
            {
                Position = 0,
                ContentType = "Text",
                Content = ""
            };

            contents.Add(content);
            var result = await _postsController.Create(17, post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Create", result.ActionName);
            Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        }

        //TS09-1 +
        [Test, Order(5)]
        public async Task TS09_1()
        {
            int toChange = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toChange)
                {
                    toChange = posts.Id;
                }
            }

            Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
            post.Title = "DS1 was great";
            post.Game = "Dark Souls 1";

            List<PostContentViewModel> contents = new List<PostContentViewModel>();
            PostContentViewModel content = new PostContentViewModel()
            {
                Position = 0,
                ContentType = "Text",
                Content = "Dark Sousl 1 was my first soulslike games, and I like it"
            };

            contents.Add(content);
            var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

        }
        //TS09-2 +
        [Test, Order(6)]
        public async Task TS09_2()
        {
            int toChange = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toChange)
                {
                    toChange = posts.Id;
                }
            }
            Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
            post.Title = "DS1 was great";
            post.Game = "Dark Souls 1";

            List<PostContentViewModel> contents = new List<PostContentViewModel>();
            PostContentViewModel content = new PostContentViewModel()
            {
                Position = 0,
                ContentType = "Image",
                Content = Convert.ToBase64String(MyConvert.ConvertFileToByteArray(CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png")))
            };

            contents.Add(content);
            var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }
        //TS09-3 +
        [Test, Order(7)]
        public async Task TS09_3()
        {
            int toChange = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toChange)
                {
                    toChange = posts.Id;
                }
            }

            Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
            post.Title = "DS1 was great";
            post.Game = "Dark Souls 1";

            List<PostContentViewModel> contents = new List<PostContentViewModel>();
            PostContentViewModel content = new PostContentViewModel()
            {
                Position = 0,
                ContentType = "Video",
                Content = Convert.ToBase64String(MyConvert.ConvertFileToByteArray(CreateIFormFileFromPath("D:\\Смотреть\\записи\\Captures\\video.mp4")))
            };

            contents.Add(content);
            var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

        }
        //TS07-9 -
        [Test, Order(8)]
        public async Task TS09_4()
        {
            int toChange = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toChange)
                {
                    toChange = posts.Id;
                }
            }

            Post post = _dbContext.Posts.FirstOrDefault(post => post.Id == toChange);
            post.Title = "";
            post.Game = "Dark Souls 1";

            List<PostContentViewModel> contents = new List<PostContentViewModel>();
            PostContentViewModel content = new PostContentViewModel()
            {
                Position = 0,
                ContentType = "Text",
                Content = ""
            };

            contents.Add(content);
            var result = await _postsController.Edit(post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Edit", result.ActionName);
            Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        }

        //TS08
        [Test, Order(9)]
        public async Task TS08()
        {
            int toDelete = 0;
            foreach (var posts in _dbContext.Posts)
            {
                if (posts.Id > toDelete)
                {
                    toDelete = posts.Id;
                }
            }
            var result = await _postsController.Delete(toDelete) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }


        [Test,Order(10)]
        public async Task TS01()
        {
            var result = await _postsController.GetPosts(1, 2, null, null, null);

            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = (JsonResult)result;
            var posts = (List<PostViewModel>)jsonResult.Value;
            
            Assert.That(posts.Count, Is.EqualTo(_dbContext.Posts.Count() >= 2 ? 2 : _dbContext.Posts.Count()));

        }


        //TS18-1
        [Test]
        public async Task TS18_1()
        {
            var result = await _postsController.GetPosts(1, 2, "Elden Ring", null, null) as JsonResult;

            var posts = result.Value as List<PostViewModel>;

            Assert.AreEqual(posts.Count, _dbContext.Posts.Where(post => post.Game == "Elden Ring").Count() > 2 ? 2:_dbContext.Posts.Where(post => post.Game == "Elden Ring").Count());
        }
        //TS18-2
        [Test]
        public async Task TS18_2()
        {
            var result = await _postsController.GetPosts(1, 2, null, "Theories", null) as JsonResult;

            var posts = result.Value as List<PostViewModel>;

            var posts2 = _dbContext.Posts.Where(post => _dbContext.Blogs
                .Where(blog => blog.Id == post.BlogId)
                .Select(blog => blog.Theme)
                .FirstOrDefault() == "Theories");

            Assert.AreEqual(posts.Count, posts2.Count() > 2 ? 2 : posts2.Count());
        }
        //TS18-3
        [Test]
        public async Task TS18_3()
        {
            var result = await _postsController.GetPosts(1, 2, null, null, null) as JsonResult;

            var posts = result.Value as List<PostViewModel>;

            Assert.AreEqual(posts.Count, _dbContext.Posts.Where(post => post.Game == "Elden Ring").Count() > 2 ? 2 : _dbContext.Posts.Where(post => post.Game == "Elden Ring").Count());
        }
        //TS18-4
        [Test]
        public async Task TS18_4()
        {
            var result = await _postsController.GetPosts(1, 2, null, null, "Dark Souls") as JsonResult;

            var posts = result.Value as List<PostViewModel>;

            Assert.AreEqual(posts.Count, _dbContext.Posts.Where(post => post.Title.Contains("Dark Souls")).Count() > 2 ? 2 : _dbContext.Posts.Where(post => post.Title.Contains("Dark Souls")).Count());
        }



        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _postsController?.Dispose();
        }
    }
}
