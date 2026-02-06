using Microsoft.EntityFrameworkCore;
using FIAP.Agro.Property.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FIAP.Agro.Property.Infrastructure.Data
{
    public class PropertyDbContext : DbContext
    {
        public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options) { }

        public DbSet<FarmProperty> Properties => Set<FarmProperty>();
        public DbSet<Field> Fields => Set<Field>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FarmProperty>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
                e.Property(x => x.Location).HasMaxLength(200);

                e.HasMany(x => x.Fields)
                 .WithOne(x => x.Property)
                 .HasForeignKey(x => x.PropertyId);
            });

            modelBuilder.Entity<Field>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
                e.Property(x => x.Crop).IsRequired().HasMaxLength(80);
                e.Property(x => x.GeoJson).HasColumnType("nvarchar(max)");
            });
        }
    }
}
