using Microsoft.EntityFrameworkCore;
using FIAP.Agro.Alert.Domain;

namespace FIAP.Agro.Alert.Infrastructure.Data
{
    public class AlertDbContext : DbContext
    {
        public AlertDbContext(DbContextOptions<AlertDbContext> options) : base(options) { }
        public DbSet<AlertModel> Alerts => Set<AlertModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlertModel>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.FieldId, x.Type, x.Status });
                e.Property(x => x.Type).HasMaxLength(50).IsRequired();
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();
                e.Property(x => x.Message).HasMaxLength(300).IsRequired();
            });
        }
    }
}
