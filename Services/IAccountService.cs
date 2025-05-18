// Services/IAccountService.cs
using Microsoft.AspNetCore.Identity;
using mvc.Models;
using mvc.ViewModels;

namespace mvc.Services;
public interface IAccountService
{
    Task<UserAccount?> AuthenticateUserAsync(string email, string password);
    Task<IdentityResult> RegisterUserAsync(RegisterViewModel model);
    Task LogoutAsync();
    Task AddUserRoleAsync(string email, string role);
    Task<IEnumerable<UserAccount>> GetUsersByAccountTypeAsync(string accountType);
}