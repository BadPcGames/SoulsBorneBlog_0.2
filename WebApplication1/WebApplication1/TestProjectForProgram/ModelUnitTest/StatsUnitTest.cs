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


namespace ModelTest
{

    [TestFixture]
    public class StatsUnitTest
    {
        private StatsController _statsController;
        private EmailService _emailService;
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
            _userService=new UserService(_dbContext, _contextAccessor);
            _statsController = new StatsController(_dbContext);
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        
        //TS27-6
        [Test]
        public async Task TS27_6()
        {
            var result = await _statsController.GetFileStats("02.01.2025", "07.03.2025","ex") as FileContentResult;

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        }
        //TS27-7
        [Test]
        public async Task TS27_7()
        {
            var result = await _statsController.GetFileStats("02.01.2025", "07.03.2025", "pdf") as FileContentResult;

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual("application/pdf", result.ContentType);
        }




        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
            _statsController?.Dispose();        
        }
    }
}
