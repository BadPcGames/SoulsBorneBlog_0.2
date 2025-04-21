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
using Microsoft.Extensions.Options;

namespace SysteamTest
{
    [TestFixture]
    public class SysteamAuthUnitTest
    {
        private AuthController _authController;
        private HomeController _homeController;
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
            _emailService = new EmailService();
            var adminEmailOptions = _config.GetSection("AdminEmail").Get<AdminEmailOptions>();
            _adminEmail = Options.Create(adminEmailOptions);
            _authController = new AuthController(_dbContext, _config, _userService, _emailService, _adminEmail);
            _homeController = new HomeController(_dbContext);
        }



        public async Task LoginUsers(int value)
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

            for (int i = 0; i < value; i++)
            {
                var user = new LoginModel
                {
                    Email = "nf" + i + "@gmail.com",
                    Password = "123456"
                };

                var result = await _authController.Login(user);
            }
        }

        public async Task Register(int value)
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

            for (int i = 0; i < value; i++)
            {
                var user = new RegisterModel
                {
                    Name = "nf" + i,
                    Email = "nf" + i + "@gmail.com",
                    Password = "123456"
                };

                var result = await _authController.Register(user);
            }
        }



        //TS02-1 -
        [Test]
        public async Task TS02_1()
        {
            var user = new RegisterModel
            {
                Name = "test",
                Email = "test",
                Password = "123456"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("email is incorrect", result.RouteValues["message"]);
        }

        //TS02-2 +
        [Test]
        public async Task TS02_2()
        {
            var user = new RegisterModel
            {
                Name = "test",
                Email = "test@gmail.com",
                Password = "123456"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Profile", result.ControllerName);

            var userToDelete = _dbContext.Users.FirstOrDefault(x => x.Email == user.Email);
            _dbContext.Users.Remove(userToDelete);
            _dbContext.SaveChanges();
        }

        //TS02-2 +
        [Test]
        public async Task TS17_1()
        {
            var user = new RegisterModel
            {
                Name = "test5000",
                Email = "test5000@gmail.com",
                Password = "123456"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Profile", result.ControllerName);

            var userToDelete = _dbContext.Users.FirstOrDefault(x => x.Email == user.Email);
            _dbContext.Users.Remove(userToDelete);
            _dbContext.SaveChanges();
        }


        //TS02-3 -
        [Test]
        public async Task TS02_3()
        {
            var user = new RegisterModel
            {
                Name = "test",
                Email = "user1@gmail.com",
                Password = "123456"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("email is alredy used", result.RouteValues["message"]);
        }

        //TS02-4 -
        [Test]
        public async Task TS02_4()
        {
            var user = new RegisterModel
            {
                Name = "test",
                Email = "",
                Password = "123456"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("all data must be fielded", result.RouteValues["message"]);
        }

        //TS02-5 -
        [Test]
        public async Task TS02_5()
        {
            var user = new RegisterModel
            {
                Name = "test",
                Email = "test@gmail.com",
                Password = "123"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("password is too short", result.RouteValues["message"]);
        }



        //TS03-1
        [Test]
        public async Task TS03_1()
        {
            var user = new LoginModel
            {
                Email = "test",
                Password = "123456"
            };

            var result = await _authController.Login(user) as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("email is incorrect", result.RouteValues["message"]);
        }

        //TS03-2
        [Test]
        public async Task TS03_2()
        {
            var user = new LoginModel
            {
                Email = "user1@gmail.com",
                Password = "123456"
            };

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

            var result = await _authController.Login(user);

            Assert.IsInstanceOf<RedirectToActionResult>(result);
        }

        //TS03-3
        [Test]
        public async Task TS03_3()
        {
            var user = new LoginModel
            {
                Email = "user1@gmail.com",
                Password = "1234564732"
            };

            var result = await _authController.Login(user) as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("password is incorrect", result.RouteValues["message"]);
        }
        //TS03-4
        [Test]
        public async Task TS03_4()
        {
            var user = new LoginModel
            {
                Email = "",
                Password = "123456"
            };

            var result = await _authController.Login(user) as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual("Index", result.ActionName);
            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("all data must be fielded", result.RouteValues["message"]);
        }



        //TS14-1
        [Test]
        public async Task TS14_1()
        {
            LoginUsers(100);

            DateTime first = DateTime.Now;

            await _homeController.Index();
            DateTime second = DateTime.Now;

            Assert.Greater(first.AddSeconds(2), second);
        }

        //TS14-2
        [Test]
        public async Task TS14_2()
        {
            LoginUsers(2500);

            DateTime first = DateTime.Now;

            await _homeController.Index();
            DateTime second = DateTime.Now;

            Assert.Greater(first.AddSeconds(4), second);
        }


        //TS29-1
        [Test]
        public async Task TS29_1()
        {
            var user = new RegisterModel
            {
                Name = "test",
                Email = "test@gmail.com",
                Password = "123456"
            };

            var result = await _authController.Register(user) as RedirectToActionResult;

            Assert.AreNotEqual("123456", _dbContext.Users.FirstOrDefault(u => u.Name == "test").PasswordHash);

            var userToDelete = _dbContext.Users.FirstOrDefault(x => x.Email == user.Email);
            _dbContext.Users.Remove(userToDelete);
            _dbContext.SaveChanges();
        }

        //TS29-2
        [Test]
        public async Task TS029_2()
        {
            var user = new LoginModel
            {
                Email = "user1@gmail.com",
                Password = _dbContext.Users.FirstOrDefault(u=>u.Email== "user1@gmail.com").PasswordHash
            };

            var result = await _authController.Login(user) as RedirectToActionResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);

            Assert.IsTrue(result.RouteValues.ContainsKey("message"));
            Assert.AreEqual("password is incorrect", result.RouteValues["message"]);
        }


        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _authController?.Dispose();
            _homeController?.Dispose();
        }
    }
}
