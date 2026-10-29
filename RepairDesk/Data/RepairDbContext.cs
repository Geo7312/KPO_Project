using System.IO;
using Microsoft.EntityFrameworkCore;
using RepairDesk.Domain;

namespace RepairDesk.Data;

public sealed class RepairDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<RepairOrder> Orders => Set<RepairOrder>();
    public DbSet<RepairWork> Works => Set<RepairWork>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<PartUsage> PartUsages => Set<PartUsage>();

    public static string DatabasePath
    {
        get
        {
            var overridePath = Environment.GetEnvironmentVariable("REPAIRDESK_DB_PATH");
            if (!string.IsNullOrWhiteSpace(overridePath)) return Path.GetFullPath(overridePath);
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RepairDesk");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "repair.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DatabasePath}");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<RepairOrder>().Ignore(x => x.Total).Ignore(x => x.Number).Ignore(x => x.StatusName).Ignore(x => x.CustomerName).Ignore(x => x.DeviceName);
        model.Entity<PartUsage>().Ignore(x => x.PartName).Ignore(x => x.Sum);
        model.Entity<SparePart>().Property(x => x.Price).HasConversion<string>();
        model.Entity<RepairWork>().Property(x => x.Price).HasConversion<string>();
        model.Entity<PartUsage>().Property(x => x.UnitPrice).HasConversion<string>();
        model.Entity<SparePart>().HasIndex(x => x.Article).IsUnique();
        model.Entity<PartUsage>().HasOne(x => x.SparePart).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
