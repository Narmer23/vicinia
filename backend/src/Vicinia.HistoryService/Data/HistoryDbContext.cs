using Microsoft.EntityFrameworkCore;
using Vicinia.HistoryService.Models;

namespace Vicinia.HistoryService.Data;

public class HistoryDbContext : DbContext
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options) : base(options)
    {
    }

    public DbSet<SearchHistory> SearchHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TransportationMode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.OverallScore).IsRequired();
            entity.Property(e => e.SearchDate).IsRequired();
            entity.Property(e => e.PoiCount).IsRequired();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SearchDate);
        });
    }
} 