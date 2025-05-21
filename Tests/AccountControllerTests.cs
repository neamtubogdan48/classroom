using Microsoft.AspNetCore.Mvc;
using Moq;
using mvc.Controllers;
using mvc.Models;
using mvc.Services;
using mvc.ViewModels;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using NuGet.ContentModel;

namespace mvc.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();

        private AccountController CreateController()
        {
            return new AccountController(_accountServiceMock.Object, _notificationServiceMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_RedirectsToHome()
        {
            _accountServiceMock.Setup(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserAccount());
            var controller = CreateController();
            var result = await controller.Login(new LoginViewModel { Email = "test@example.com", Password = "correct" });
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
        {
            _accountServiceMock.Setup(s => s.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UserAccount)null!);
            var controller = CreateController();
            var result = await controller.Login(new LoginViewModel { Email = "test@example.com", Password = "wrong" });
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Register_Post_SuccessfulRegistration_RedirectsToLogin()
        {
            _accountServiceMock.Setup(s => s.RegisterUserAsync(It.IsAny<RegisterViewModel>()))
                .ReturnsAsync(IdentityResult.Success);
            _accountServiceMock.Setup(s => s.AddUserRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            var controller = CreateController();
            var model = new RegisterViewModel { Email = "test@example.com", Password = "pass", accountType = "Student" };
            var result = await controller.Register(model);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }

        [Fact]
        public async Task Register_Post_FailedRegistration_ReturnsViewWithErrors()
        {
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Error" });
            _accountServiceMock.Setup(s => s.RegisterUserAsync(It.IsAny<RegisterViewModel>()))
                .ReturnsAsync(identityResult);
            var controller = CreateController();
            var model = new RegisterViewModel { Email = "test@example.com", Password = "pass", accountType = "Student" };
            var result = await controller.Register(model);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task RequestReset_Post_ValidEmail_SendsNotificationsAndRedirects()
        {
            var admins = new List<UserAccount> { new UserAccount { Id = "admin1" } };
            _accountServiceMock.Setup(s => s.GetUsersByAccountTypeAsync("Admin")).ReturnsAsync(admins);
            _notificationServiceMock.Setup(s => s.AddNotificationAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);
            var controller = CreateController();
            var result = await controller.RequestReset("test@example.com");
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }

        [Fact]
        public async Task RequestReset_Post_EmptyEmail_ReturnsViewWithError()
        {
            var controller = CreateController();
            var result = await controller.RequestReset("");
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}