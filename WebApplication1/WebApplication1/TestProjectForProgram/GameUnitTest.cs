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

namespace AuthUnitTest
{
    [TestFixture]
    public class GameUnitTest
    {
        //private AuthController _authController;
        //private AppDbContext _dbContext;
        //private IConfiguration _config;

        //[SetUp]
        //public void Setup()
        //{
        //    var configuration = new ConfigurationBuilder()
        //        .SetBasePath(AppContext.BaseDirectory)  // Указывает путь на bin\Debug\net8.0
        //        .AddJsonFile("TestAppSettings.json", optional: false, reloadOnChange: true)
        //        .Build();

        //    _config = configuration;

        //    var options = new DbContextOptionsBuilder<AppDbContext>()
        //        .UseMySql(
        //            _config.GetConnectionString("DefaultConnection"),
        //            ServerVersion.AutoDetect(_config.GetConnectionString("DefaultConnection"))
        //        )
        //        .Options;

        //    _dbContext = new AppDbContext(options);
        //    _authController = new AuthController(_dbContext, _config);
        //}




        ////TS02-1 -
        //[Test]
        //public async Task TS02_1()
        //{
        //    var user = new RegisterModel
        //    {
        //        Name = "test",
        //        Email = "test",
        //        Password = "123456"
        //    };

        //    var result = await _authController.Register(user) as RedirectToActionResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("email is incorrect", result.RouteValues["message"]);
        //}

        ////TS02-2 +
        //[Test]
        //public async Task TS02_2()
        //{
        //    var user = new RegisterModel
        //    {
        //        Name = "test",
        //        Email = "test@gmail.com",
        //        Password = "123456"
        //    };

        //    var result = await _authController.Register(user) as RedirectToActionResult;
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);
        //    Assert.AreEqual("Profile", result.ControllerName);

        //    var userToDelete = _dbContext.Users.FirstOrDefault(x => x.Email == user.Email);
        //    _dbContext.Users.Remove(userToDelete);
        //    _dbContext.SaveChanges();

        //}

        ////TS02-3 -
        //[Test]
        //public async Task TS02_3()
        //{
        //    var user = new RegisterModel
        //    {
        //        Name = "test",
        //        Email = "user1@gmail.com",
        //        Password = "123456"
        //    };

        //    var result = await _authController.Register(user) as RedirectToActionResult;
        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("email is alredy used", result.RouteValues["message"]);
        //}

        ////TS02-4 -
        //[Test]
        //public async Task TS02_4()
        //{
        //    var user = new RegisterModel
        //    {
        //        Name = "test",
        //        Email = "",
        //        Password = "123456"
        //    };

        //    var result = await _authController.Register(user) as RedirectToActionResult;
          
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("all data must be fielded", result.RouteValues["message"]);
        //}

        ////TS02-5 -
        //[Test]
        //public async Task TS02_5()
        //{
        //    var user = new RegisterModel
        //    {
        //        Name = "test",
        //        Email = "test@gmail.com",
        //        Password = "123"
        //    };

        //    var result = await _authController.Register(user) as RedirectToActionResult;
        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("password is too short", result.RouteValues["message"]);
        //}



        ////TS03-1
        //[Test]
        //public async Task TS03_1()
        //{
        //    var user = new LoginModel
        //    {
        //        Email = "test",
        //        Password = "123456"
        //    };

        //    var result = await _authController.Login(user) as RedirectToActionResult;
        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("email is incorrect", result.RouteValues["message"]);
        //}

        ////TS03-2
        //[Test]
        //public async Task TS03_2()
        //{
        //    var user = new LoginModel
        //    {
        //        Email = "user1@gmail.com",
        //        Password = "123456"
        //    };

        //    // 1. Мокаем HttpContext и сервисы
        //    var httpContextMock = new Mock<HttpContext>();
        //    var serviceProviderMock = new Mock<IServiceProvider>();
        //    var urlHelperMock = new Mock<IUrlHelper>();
        //    var authServiceMock = new Mock<IAuthenticationService>();

        //    // 2. Добавляем IAuthenticationService в ServiceProvider
        //    serviceProviderMock.Setup(x => x.GetService(typeof(IAuthenticationService)))
        //                       .Returns(authServiceMock.Object);

        //    serviceProviderMock.Setup(x => x.GetService(typeof(IUrlHelperFactory)))
        //                       .Returns(new Mock<IUrlHelperFactory>().Object);

        //    httpContextMock.Setup(x => x.RequestServices)
        //                   .Returns(serviceProviderMock.Object);

        //    // 3. Присваиваем мок в контроллер
        //    _authController.ControllerContext = new ControllerContext
        //    {
        //        HttpContext = httpContextMock.Object
        //    };

        //    _authController.Url = urlHelperMock.Object;

        //    // 4. Мокаем SignInAsync, чтобы избежать исключения
        //    authServiceMock.Setup(x => x.SignInAsync(It.IsAny<HttpContext>(),
        //                                             It.IsAny<string>(),
        //                                             It.IsAny<ClaimsPrincipal>(),
        //                                             It.IsAny<AuthenticationProperties>()))
        //                   .Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _authController.Login(user);

        //    // Assert
        //    Assert.IsInstanceOf<RedirectToActionResult>(result);
        //}
        ////TS03-3
        //[Test]
        //public async Task TS03_3()
        //{
        //    var user = new LoginModel
        //    {
        //        Email = "user1@gmail.com",
        //        Password = "1234564732"
        //    };

        //    var result = await _authController.Login(user) as RedirectToActionResult;
        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual("Index", result.ActionName);

        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("password is incorrect", result.RouteValues["message"]);
        //}
        ////TS03-4
        //[Test]
        //public async Task TS03_4()
        //{
        //    var user = new LoginModel
        //    {
        //        Email = "",
        //        Password = "123456"
        //    };

        //    var result = await _authController.Login(user) as RedirectToActionResult;
        //    // Assert
        //    Assert.IsNotNull(result);

        //    Assert.AreEqual("Index", result.ActionName);
        //    Assert.IsTrue(result.RouteValues.ContainsKey("message"));
        //    Assert.AreEqual("all data must be fielded", result.RouteValues["message"]);
        //}

        //[TearDown]
        //public void TearDown()
        //{
        //    _dbContext?.Dispose();
        //    _authController?.Dispose();
        //}
    }
}
