using Microsoft.EntityFrameworkCore;
using FIAP.Agro.Ingestion.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FIAP.Agro.Ingestion.Infrastructure.Data
{
    public class IngestionDbContext : DbContext
    {
        public IngestionDbContext(DbContextOptions<IngestionDbContext> options) : base(options) { }

        public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorReading>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.FieldId, x.Timestamp });
                e.Property(x => x.SoilMoisture).HasColumnType("decimal(5,2)");
                e.Property(x => x.Temperature).HasColumnType("decimal(5,2)");
                e.Property(x => x.RainfallMm).HasColumnType("decimal(7,2)");
            });
        }
    }
}