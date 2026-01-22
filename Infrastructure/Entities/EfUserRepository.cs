using Domain.Auth;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class EfUserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;


    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        return (user == null)
            ? null
            : new ApplicationUser { Id = user.Id, Email = user.Email, PasswordHash = user.PasswordHash };
    }

    public async Task<ApplicationUser?> FindByIdAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        return (user == null)
            ? null
            : new ApplicationUser { Id = user.Id, Email = user.Email, PasswordHash = user.PasswordHash };
    }

    public async Task CreateAsync(ApplicationUser user)
    {
        var newUser = new UserEntity {Id = user.Id, Email = user.Email, PasswordHash = user.PasswordHash};
        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();
    }
}
