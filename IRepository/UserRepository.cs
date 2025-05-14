using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using mvc.Data;
using mvc.Models;

namespace mvc.IRepository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAccount>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<UserAccount?> GetUserByIdAsync(string id)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
    }
    public async Task<UserAccount?> GetUserByEmailAsync(string normalizedEmail)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
    }

    public async Task AddUserAsync(UserAccount user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UserAccount user)
    {
        var existingUser = await _context.Users.FindAsync(user.Id);
        if (existingUser == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Update only the necessary fields
        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.accountType = user.accountType;
        existingUser.notificationSettings = user.notificationSettings;
        existingUser.githubLink = user.githubLink;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.profilePhoto = user.profilePhoto;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(UserAccount user)
    {
        // Remove user roles
        var userRoles = _context.UserRoles.Where(ur => ur.UserId == user.Id);
        _context.UserRoles.RemoveRange(userRoles);

        // Remove the user
        _context.Users.Remove(user);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task AddUserRoleAsync(string userId, string roleId)
    {
        var userRole = new IdentityUserRole<string>
        {
            UserId = userId,
            RoleId = roleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserRoleAsync(UserAccount user, string newRole)
    {
        // Fetch the current roles of the user
        var currentRoles = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync();

        // Remove existing roles
        if (currentRoles.Any())
        {
            _context.UserRoles.RemoveRange(currentRoles);
        }

        // Fetch the role ID for the new role
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == newRole);
        if (role == null)
        {
            throw new Exception($"Role '{newRole}' does not exist.");
        }

        // Add the new role
        var newUserRole = new IdentityUserRole<string>
        {
            UserId = user.Id,
            RoleId = role.Id
        };
        _context.UserRoles.Add(newUserRole);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserAccount>> GetByAccountTypeAsync(string accountType)
    {
        return await _context.Users
            .Where(u => u.accountType == accountType)
            .ToListAsync();
    }
}
