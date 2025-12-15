using Clean.Architecture.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Clean.Architecture.Persistence.Users;

public sealed class EfRoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public EfRoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .ToListAsync(cancellationToken);
        return roles.AsReadOnly();
    }
}
