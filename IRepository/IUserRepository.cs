// IRepository/IUserRepository.cs
using mvc.Models;

namespace mvc.IRepository;

public interface IUserRepository
{
    Task<IEnumerable<UserAccount>> GetAllUsersAsync();
    Task<UserAccount?> GetUserByIdAsync(string id);
    Task<UserAccount?> GetUserByEmailAsync(string normalizedEmail); // New method
    Task AddUserAsync(UserAccount user);
    Task UpdateUserAsync(UserAccount user);
    Task DeleteUserAsync(UserAccount user);
    Task AddUserRoleAsync(string userId, string roleId);
    Task UpdateUserRoleAsync(UserAccount user, string newRole);
}

