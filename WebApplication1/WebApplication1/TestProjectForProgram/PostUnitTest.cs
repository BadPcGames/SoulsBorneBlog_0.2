using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication1;
using WebApplication1.Controllers;
using WebApplication1.Models;
using WebApplication1.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthUnitTest
{

    [TestFixture]
    public class PostUnitTest
    {

        public IFormFile CreateIFormFileFromPath(string filePath)
        {
            var stream = new MemoryStream(File.ReadAllBytes(filePath));
            return new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(filePath));
        }

        private PostsController _postsController;
        private AppDbContext _dbContext;
        private IConfiguration _config;

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
            _postsController = new PostsController(_dbContext);
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
                ContentType= "Text",
                Content= "Dark Sousl 1 was my first soulslike games, and I like it"
            };

            contents.Add(content);
            //IFormFile file = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png");
            var result = await _postsController.Create(17,post,contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            int toDelete = 0;
            foreach(var posts in _dbContext.Posts)
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

            //int toDelete = 0;
            //foreach (var posts in _dbContext.Posts)
            //{
            //    if (posts.Id > toDelete)
            //    {
            //        toDelete = posts.Id;
            //    }
            //}
            //await _postsController.Delete(toDelete);
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
                Content=""
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
                FormFile = CreateIFormFileFromPath("D:\\git\\NewProjectWithSqlForDiplom\\WebApplication1\\WebApplication1\\WebApplication1\\wwwroot\\images\\images.png")
            };

            contents.Add(content);
            var result = await _postsController.Edit( post, contents) as RedirectToActionResult;

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
                FormFile = CreateIFormFileFromPath("D:\\Смотреть\\записи\\Captures\\video.mp4")
            };

            contents.Add(content);
            var result = await _postsController.Edit( post, contents) as RedirectToActionResult;

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
            var result = await _postsController.Edit( post, contents) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Edit", result.ActionName);
            Assert.AreEqual("all data must be fuiled", result.RouteValues["message"]);
        }

        //TS08
        [Test,Order(9)]
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
            var result=await _postsController.Delete(toDelete) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _postsController?.Dispose();
        }
    }
}
