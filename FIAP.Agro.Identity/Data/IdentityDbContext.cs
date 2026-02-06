using Microsoft.EntityFrameworkCore;
using FIAP.Agro.Identity.Models;

namespace FIAP.Agro.Identity.Data
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = default!;
    }
}