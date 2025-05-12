using mvc.IRepository;
using mvc.Models;
using mvc.ViewModels;

namespace mvc.Services
{
    public class ClassroomService : IClassroomService
    {
        private readonly IClassroomRepository _classroomRepository;
        private readonly IUserService _userService;

        public ClassroomService(IClassroomRepository classroomRepository, IUserService userService)
        {
            _classroomRepository = classroomRepository;
            _userService = userService;
        }

        public async Task<IEnumerable<Classroom>> GetAllClassroomsAsync()
        {
            return await _classroomRepository.GetAllClassroomsAsync();
        }

        public async Task<Classroom?> GetClassroomByIdAsync(int id)
        {
            return await _classroomRepository.GetClassroomByIdAsync(id);
        }

        public async Task AddClassroomAsync(Classroom classroom)
        {
            await _classroomRepository.AddClassroomAsync(classroom);
        }

        public async Task UpdateClassroomAsync(Classroom classroom)
        {
            await _classroomRepository.UpdateClassroomAsync(classroom);
        }

        public async Task DeleteClassroomAsync(int id)
        {
            await _classroomRepository.DeleteClassroomAsync(id);
        }

        public async Task<Classroom?> GetClassroomByCodeAsync(int code)
        {
            return await _classroomRepository.GetClassroomByCodeAsync(code);
        }

        public async Task<List<ClassroomViewModel>> MapClassroomsToViewModelsAsync(IEnumerable<Classroom> classrooms)
        {
            var classroomViewModels = new List<ClassroomViewModel>();

            foreach (var classroom in classrooms)
            {
                if (!string.IsNullOrEmpty(classroom.professorId))
                {
                    var professor = await _userService.GetUserByIdAsync(classroom.professorId);
                    classroomViewModels.Add(new ClassroomViewModel
                    {
                        Classroom = classroom,
                        ProfessorName = professor?.UserName,
                        ProfessorPhoto = professor?.profilePhoto
                    });
                }
            }

            return classroomViewModels;
        }
    }
}
