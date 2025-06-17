using Microsoft.EntityFrameworkCore;
using Vicinia.LoggingService.Models;

namespace Vicinia.LoggingService.Data;

public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogLevel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();

            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.LogLevel);
            entity.HasIndex(e => e.UserId);
        });
    }
} 