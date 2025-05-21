using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using mvc.Controllers;
using mvc.Models;
using mvc.Services;
using Xunit;

namespace mvc.Tests
{
    public class AssignmentChatControllerTests
    {
        private readonly Mock<IAssignmentChatService> _assignmentChatServiceMock = new();
        private readonly Mock<IAssignmentService> _assignmentServiceMock = new();
        private readonly Mock<UserManager<UserAccount>> _userManagerMock;

        public AssignmentChatControllerTests()
        {
            var store = new Mock<IUserStore<UserAccount>>();
            _userManagerMock = new Mock<UserManager<UserAccount>>(
                store.Object, null, null, null, null, null, null, null, null
            );
        }

        private AssignmentChatController CreateController()
        {
            return new AssignmentChatController(
                _userManagerMock.Object,
                _assignmentChatServiceMock.Object,
                _assignmentServiceMock.Object
            );
        }

        [Fact]
        public async Task Create_ValidChatMessage_RedirectsToClassroomFlux()
        {
            // Arrange
            var chat = new AssignmentChat
            {
                message = "Hello!",
                userId = "user1",
                studentId = "student1",
                assignmentId = 42
            };
            var assignment = new Assignment
            {
                id = 42,
                classroomId = 7
            };

            _assignmentChatServiceMock.Setup(s => s.AddAssignmentChatAsync(chat)).Returns(Task.CompletedTask);
            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(chat.assignmentId)).ReturnsAsync(assignment);

            var controller = CreateController();

            // Act
            var result = await controller.Create(chat);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ClassroomFlux", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal(7, redirect.RouteValues["id"]);
        }
    }
}
