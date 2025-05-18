// Services/AccountService.cs
using Microsoft.AspNetCore.Identity;
using mvc.IRepository;
using mvc.Models;
using mvc.ViewModels;

namespace mvc.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserAccount> _userManager;
    private readonly SignInManager<UserAccount> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountService(IUserRepository userRepository, UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, RoleManager<IdentityRole> roleManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;

    }

    public async Task<UserAccount?> AuthenticateUserAsync(string email, string password)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _userRepository.GetUserByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (result.Succeeded)
        {
            // Sign the user in after successful password check
            await _signInManager.SignInAsync(user, isPersistent: false);
            return user;
        }

        return null;
    }

    public async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model)
    {
        var user = new UserAccount
        {
            UserName = model.Username,
            Email = model.Email,
            githubLink = model.githubLink,
            accountType = model.accountType,
            notificationSettings = model.notificationSettings,
            PhoneNumber = model.PhoneNumber,
            profilePhoto = model.profilePhoto
        };


        return await _userManager.CreateAsync(user, model.Password);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task AddUserRoleAsync(string email, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception($"User with email '{email}' not found.");
        }

        var roleExists = await _roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            throw new Exception($"Role '{role}' does not exist.");
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to add role '{role}' to user '{email}'.");
        }
    }

    public async Task<IEnumerable<UserAccount>> GetUsersByAccountTypeAsync(string accountType)
    {
        return await _userRepository.GetByAccountTypeAsync(accountType);
    }
}
