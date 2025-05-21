using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using mvc.Controllers;
using mvc.Models;
using mvc.Services;
using Xunit;

namespace mvc.Tests
{
    public class DocumentControllerTests
    {
        private readonly Mock<IDocumentService> _documentServiceMock = new();
        private readonly Mock<IClassroomStudentsService> _classroomStudentsServiceMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<IAssignmentService> _assignmentServiceMock = new();
        private readonly Mock<INotificationService> _notificationServiceMock = new();
        private readonly Mock<UserManager<UserAccount>> _userManagerMock;

        public DocumentControllerTests()
        {
            // Setup UserManager mock
            var store = new Mock<IUserStore<UserAccount>>();
            _userManagerMock = new Mock<UserManager<UserAccount>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private DocumentController CreateController()
        {
            var controller = new DocumentController(
                _userManagerMock.Object,
                _documentServiceMock.Object,
                _classroomStudentsServiceMock.Object,
                _userServiceMock.Object,
                _assignmentServiceMock.Object
            );

            var httpContext = new DefaultHttpContext();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<INotificationService>(_notificationServiceMock.Object);

            // Add this line to register IUrlHelperFactory
            serviceCollection.AddSingleton<IUrlHelperFactory>(new Mock<IUrlHelperFactory>().Object);

            httpContext.RequestServices = serviceCollection.BuildServiceProvider();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        [Fact]
        public async Task GradeDocument_ValidDocument_UpdatesGradeAndSendsNotification()
        {
            // Arrange
            int documentId = 1;
            int assignmentId = 5;
            int grade = 85;
            string userId = "student1";

            var document = new Document 
            { 
                id = documentId,
                assignmentId = assignmentId,
                userId = userId,
                grade = 0 // Initial grade is 0
            };

            var assignment = new Assignment 
            { 
                id = assignmentId,
                name = "Test Assignment"
            };

            _documentServiceMock.Setup(s => s.GetDocumentByIdAsync(documentId)).ReturnsAsync(document);
            _documentServiceMock.Setup(s => s.UpdateDocumentAsync(It.IsAny<Document>())).Returns(Task.CompletedTask);
            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(assignmentId)).ReturnsAsync(assignment);
            _notificationServiceMock.Setup(s => s.AddNotificationAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Act
            var result = await controller.GradeDocument(documentId, grade);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UploadList", redirectResult.ActionName);
            Assert.Equal(assignmentId, redirectResult.RouteValues["id"]);

            _documentServiceMock.Verify(s => s.UpdateDocumentAsync(It.Is<Document>(d => 
                d.id == documentId && 
                d.grade == grade)), Times.Once);
                
            _notificationServiceMock.Verify(s => s.AddNotificationAsync(It.Is<Notification>(n => 
                n.userId == userId && 
                n.name == "Grade Received" && 
                n.description.Contains("85"))), Times.Once);
        }

        [Fact]
        public async Task ReturnDocument_ValidDocument_SendsNotificationAndDeletesDocument()
        {
            // Arrange
            int documentId = 1;
            int assignmentId = 5;
            string userId = "student1";
            int classroomId = 10;

            var document = new Document 
            { 
                id = documentId,
                assignmentId = assignmentId,
                userId = userId,
                studentDoc = null // No file to delete in test
            };

            var assignment = new Assignment 
            { 
                id = assignmentId,
                name = "Test Assignment",
                classroomId = classroomId
            };

            _documentServiceMock.Setup(s => s.GetDocumentByIdAsync(documentId)).ReturnsAsync(document);
            _documentServiceMock.Setup(s => s.DeleteDocumentAsync(documentId)).Returns(Task.CompletedTask);
            _assignmentServiceMock.Setup(s => s.GetAssignmentByIdAsync(assignmentId)).ReturnsAsync(assignment);
            _notificationServiceMock.Setup(s => s.AddNotificationAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            var controller = CreateController();

            // Act
            var result = await controller.ReturnDocument(documentId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ClassroomFlux", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal(classroomId, redirectResult.RouteValues["id"]);

            _documentServiceMock.Verify(s => s.DeleteDocumentAsync(documentId), Times.Once);
            _notificationServiceMock.Verify(s => s.AddNotificationAsync(It.Is<Notification>(n => 
                n.userId == userId && 
                n.name == "Homework Returned")), Times.Once);
        }

        [Fact]
        public async Task UploadList_ValidAssignment_ReturnsViewWithDocumentsAndStudents()
        {
            // Arrange
            int assignmentId = 5;
            
            var classroomStudents = new List<ClassroomStudents>
            {
                new ClassroomStudents { userId = "student1", classroomId = 10 },
                new ClassroomStudents { userId = "student2", classroomId = 10 }
            };
            
            var documents = new List<Document>
            {
                new Document { id = 1, assignmentId = assignmentId, userId = "student1" },
                new Document { id = 2, assignmentId = assignmentId, userId = "student2" }
            };
            
            var users = new List<UserAccount>
            {
                new UserAccount { Id = "student1", UserName = "Student One" },
                new UserAccount { Id = "student2", UserName = "Student Two" }
            };

            _classroomStudentsServiceMock.Setup(s => s.GetClassroomStudentsByAssignmentIdAsync(assignmentId))
                .ReturnsAsync(classroomStudents);
            _documentServiceMock.Setup(s => s.GetDocumentsByAssignmentIdAsync(assignmentId))
                .ReturnsAsync(documents);
            _userServiceMock.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            var controller = CreateController();

            // Act
            var result = await controller.UploadList(assignmentId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<mvc.ViewModels.ClassroomFluxViewModel>(viewResult.Model);
            
            Assert.Equal(classroomStudents, model.ClassroomStudents);
            Assert.Equal(documents, model.Documents);
            Assert.Equal(users, model.Users);
            Assert.Equal(assignmentId, viewResult.ViewData["AssignmentId"]);
        }
    }
}

