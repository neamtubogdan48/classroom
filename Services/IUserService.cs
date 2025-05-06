using Microsoft.AspNetCore.Identity;
using mvc.Models;

namespace mvc.Services;

public interface IUserService
{
    Task<IEnumerable<UserAccount>> GetAllUsersAsync();
    Task<UserAccount?> GetUserByIdAsync(string id);
    Task AddUserAsync(UserAccount user, string role);
    Task UpdateUserAsync(UserAccount user);
    Task DeleteUserAsync(UserAccount user);
}
