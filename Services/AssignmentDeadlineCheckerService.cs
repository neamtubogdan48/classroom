using mvc.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq; // Add this for LINQ methods like Select
using System.Threading;
using System.Threading.Tasks;

namespace mvc.Services
{
    public class AssignmentDeadlineCheckerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public AssignmentDeadlineCheckerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow.AddHours(3);
                var nextRun = now.Date.AddDays(1).AddHours(0).AddMinutes(0);
                var delay = nextRun - now;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                    var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
                    var classroomService = scope.ServiceProvider.GetRequiredService<IClassroomService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var classroomStudentsService = scope.ServiceProvider.GetRequiredService<IClassroomStudentsService>(); // Add this line

                    var assignments = await assignmentService.GetAllAssignmentsAsync();
                    foreach (var assignment in assignments)
                    {
                        var classroom = await classroomService.GetClassroomByIdAsync(assignment.classroomId);
                        if (classroom == null) continue;

                        var classroomStudents = await classroomStudentsService.GetClassroomStudentsByClassroomIdAsync(assignment.classroomId);
                        var studentIdsInClass = classroomStudents.Select(cs => cs.userId).ToHashSet();

                        // Fetch all users whose IDs are in studentIdsInClass
                        var students = new List<UserAccount>();
                        foreach (var studentId in studentIdsInClass)
                        {
                            var student = await userService.GetUserByIdAsync(studentId);
                            if (student != null)
                            {
                                students.Add(student);
                            }
                        }

                        foreach (var student in students)
                        {
                            if (!studentIdsInClass.Contains(student.Id))
                                continue;

                            bool hasSubmitted = await documentService.HasNotificationWithDescriptionAsync(student.Id, $"You have 24 hours left to submit assignment '{assignment.name}'.");

                            var hoursLeft = (assignment.deadline - DateTime.UtcNow.AddHours(3)).TotalHours;

                            // Check if the student has submitted a document for this assignment
                            var hasSubmittedDocument = (await documentService.GetDocumentsByAssignmentIdAsync(assignment.id))
                                .Any(doc => doc.userId == student.Id);

                            // 24h left notification
                            if (!hasSubmittedDocument && hoursLeft <= 24 && hoursLeft > -1)
                            {
                                string notify24hDescription = $"You have 24 hours left to submit assignment '{assignment.name}' in class '{classroom.name}'.";
                                bool alreadyNotified24h = await notificationService.ExistsAsync(student.Id, "24h Left Assignment Deadline", notify24hDescription);
                                if (!alreadyNotified24h)
                                {
                                    var notify24h = new Notification
                                    {
                                        userId = student.Id,
                                        name = "24h Left Assignment Deadline",
                                        description = notify24hDescription,
                                        timeSent = DateTime.UtcNow.AddHours(3),
                                    };
                                    await notificationService.AddNotificationAsync(notify24h);
                                }
                            }

                            // Missed deadline notification
                            if (!hasSubmittedDocument && DateTime.UtcNow.AddHours(3) > assignment.deadline)
                            {
                                string missedDescription = $"You missed the deadline for assignment '{assignment.name}' in class '{classroom.name}'.";
                                bool alreadyNotified = await notificationService.ExistsAsync(student.Id, "Missed Assignment Deadline", missedDescription);
                                if (!alreadyNotified)
                                {
                                    var studentNotification = new Notification
                                    {
                                        userId = student.Id,
                                        name = "Missed Assignment Deadline",
                                        description = missedDescription,
                                        timeSent = DateTime.UtcNow.AddHours(3),
                                    };
                                    await notificationService.AddNotificationAsync(studentNotification);

                                    if (!string.IsNullOrEmpty(classroom.professorId))
                                    {
                                        var user = await userService.GetUserByIdAsync(student.Id);
                                        var professorNotification = new Notification
                                        {
                                            userId = classroom.professorId,
                                            name = "Student Missed Deadline",
                                            description = $"Student '{user?.UserName ?? student.Id}' missed the deadline for assignment '{assignment.name}' in class '{classroom.name}'.",
                                            timeSent = DateTime.UtcNow.AddHours(3),
                                        };
                                        await notificationService.AddNotificationAsync(professorNotification);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
