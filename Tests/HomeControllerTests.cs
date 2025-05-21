using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using mvc.Controllers;
using mvc.Models;
using mvc.Services;
using Xunit;

namespace mvc.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<UserManager<UserAccount>> _userManagerMock;
        private readonly Mock<IAccountService> _accountServiceMock = new();
        private readonly Mock<IClassroomStudentsService> _classroomStudentsServiceMock = new();
        private readonly Mock<IAssignmentService> _assignmentServiceMock = new();
        private readonly Mock<IDocumentService> _documentServiceMock = new();
        private readonly Mock<IClassroomService> _classroomServiceMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();

        public HomeControllerTests()
        {
            var store = new Mock<IUserStore<UserAccount>>();
            _userManagerMock = new Mock<UserManager<UserAccount>>(
                store.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<UserAccount>>().Object,
                new IUserValidator<UserAccount>[0],
                new IPasswordValidator<UserAccount>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<UserAccount>>>().Object
            );
        }

        private HomeController CreateController()
        {
            var controller = new HomeController(
                _userServiceMock.Object,
                _userManagerMock.Object,
                _accountServiceMock.Object,
                _classroomStudentsServiceMock.Object,
                _assignmentServiceMock.Object,
                _documentServiceMock.Object,
                _classroomServiceMock.Object,
                _notificationServiceMock.Object
            );
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        [Fact]
        public async Task UserProfile_AuthenticatedUser_ReturnsViewWithUser()
        {
            // Arrange
            var user = new UserAccount { Id = "user1", Email = "test@example.com" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            var controller = CreateController();

            // Act
            var result = await controller.UserProfile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }

        [Fact]
        public async Task Logout_RedirectsToHomePage()
        {
            // Arrange
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(s => s.LogoutAsync()).Returns(Task.CompletedTask);

            var controller = new AccountController(
                accountServiceMock.Object,
                new Mock<INotificationService>().Object
            );
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Setup mock session
            var sessionMock = new Mock<ISession>();
            controller.ControllerContext.HttpContext.Session = sessionMock.Object;
            sessionMock.Setup(s => s.Clear());

            // Act
            var result = await controller.Logout();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }
    }
}
