using Microsoft.AspNetCore.Identity;
using mvc.IRepository;
using mvc.Models;

namespace mvc.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserAccount> _userManager;

    public UserService(IUserRepository userRepository, UserManager<UserAccount> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<UserAccount>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<UserAccount?> GetUserByIdAsync(string id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    public async Task AddUserAsync(UserAccount user, string role)
    {
        await _userRepository.AddUserAsync(user);
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            throw new Exception("Failed to assign role to the user.");
        }
    }

    public async Task UpdateUserAsync(UserAccount user)
    {
        // Update the user details
        await _userRepository.UpdateUserAsync(user);

        // Update the user's role
        if (!string.IsNullOrEmpty(user.accountType))
        {
            await _userRepository.UpdateUserRoleAsync(user, user.accountType);
        }
    }

    public async Task DeleteUserAsync(UserAccount user)
    {
        await _userRepository.DeleteUserAsync(user);
    }
}
