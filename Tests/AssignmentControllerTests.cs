using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using mvc.Controllers;
using mvc.Models;
using mvc.Services;
using Xunit;

namespace mvc.Tests
{
    public class AssignmentControllerTests
    {
        private readonly Mock<IAssignmentService> _assignmentServiceMock = new();
        private readonly Mock<UserManager<UserAccount>> _userManagerMock;
        private readonly Mock<IClassroomStudentsService> _classroomStudentsServiceMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();

        public AssignmentControllerTests()
        {
            var store = new Mock<IUserStore<UserAccount>>();
            _userManagerMock 
                = new Mock<UserManager<UserAccount>>(
                store.Object,
                null, null, null, null, null, null, null, null
            );
        }

        private AssignmentController CreateController()
        {
            var controller = new AssignmentController(
                _userManagerMock.Object,
                _assignmentServiceMock.Object
            );

            // Setup HttpContext with required services
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider
                .Setup(x => x.GetService(typeof(IClassroomStudentsService)))
                .Returns(_classroomStudentsServiceMock.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(INotificationService)))
                .Returns(_notificationServiceMock.Object);

            // Add the missing URL helper factory service
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactory.Object);

            // Add TempData provider
            var tempDataProvider = new Mock<ITempDataProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ITempDataProvider)))
                .Returns(tempDataProvider.Object);

            httpContext.RequestServices = serviceProvider.Object;
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Setup the controller with other required properties
            controller.Url = new Mock<IUrlHelper>().Object;
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);

            return controller;
        }

        [Fact]
        public async Task Edit_ValidAssignment_RedirectsToClassroomFlux()
        {
            // Arrange
            int assignmentId = 1;
            int classroomId = 5;

            var existingAssignment = new Assignment
            {
                id = assignmentId,
                name = "Original Assignment",
                description = "Original Description",
                deadline = DateTime.UtcNow.AddDays(7),
                lateTurnInOption = false,
                noDeadlineOption = false,
                classroomId = classroomId,
                requirementsDoc = "/uploads/assignments/original.pdf"
            };

            var updatedAssignment = new Assignment
            {
                id = assignmentId,
                name = "Updated Assignment",
                description = "Updated Description",
                deadline = DateTime.UtcNow.AddDays(14),
                lateTurnInOption = true,
                noDeadlineOption = false,
                classroomId = classroomId
            };

            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(assignmentId)).ReturnsAsync(existingAssignment);
            _assignmentServiceMock.Setup(s => s.UpdateAssignmentAsync(It.IsAny<Assignment>())).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Act
            var result = await controller.Edit(assignmentId, updatedAssignment, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ClassroomFlux", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal(classroomId, redirectResult.RouteValues["id"]);

            _assignmentServiceMock.Verify(s => s.UpdateAssignmentAsync(It.Is<Assignment>(a =>
                a.id == assignmentId &&
                a.name == "Updated Assignment" &&
                a.description == "Updated Description" &&
                a.lateTurnInOption == true
            )), Times.Once);
        }

        [Fact]
        public async Task Delete_ExistingAssignment_RedirectsToClassroomFlux()
        {
            // Arrange
            int assignmentId = 1;
            int classroomId = 5;

            var assignment = new Assignment
            {
                id = assignmentId,
                name = "Assignment to Delete",
                classroomId = classroomId,
                requirementsDoc = null // No file to delete in test
            };

            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(assignmentId)).ReturnsAsync(assignment);
            _assignmentServiceMock.Setup(s => s.DeleteAssignmentAsync(assignmentId)).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteConfirmed(assignmentId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ClassroomFlux", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal(classroomId, redirectResult.RouteValues["id"]);

            _assignmentServiceMock.Verify(s => s.DeleteAssignmentAsync(assignmentId), Times.Once);
        }

        [Fact]
        public async Task Edit_NonExistentAssignment_ReturnsNotFound()
        {
            // Arrange
            int nonExistentId = 999;
            var assignment = new Assignment { id = nonExistentId };

            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(nonExistentId)).ReturnsAsync((Assignment)null);

            var controller = CreateController();

            // Act
            var result = await controller.Edit(nonExistentId, assignment, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistentAssignment_RedirectsToIndex()
        {
            // Arrange
            int nonExistentId = 999;

            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(nonExistentId)).ReturnsAsync((Assignment)null);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteConfirmed(nonExistentId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
