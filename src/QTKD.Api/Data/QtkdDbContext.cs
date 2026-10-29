using Microsoft.EntityFrameworkCore;
using QTKD.Api.Entities;

namespace QTKD.Api.Data;

public class QtkdDbContext : DbContext
{
    public QtkdDbContext(DbContextOptions<QtkdDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceItem> PriceItems => Set<PriceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PriceItem>()
            .HasKey(item => new { item.PriceListId, item.ServiceCode });

        modelBuilder.Entity<PriceItem>()
            .Property(item => item.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Contract>()
            .HasOne(contract => contract.Customer)
            .WithMany(customer => customer.Contracts)
            .HasForeignKey(contract => contract.CustomerId);

        modelBuilder.Entity<PriceItem>()
            .HasOne(item => item.PriceList)
            .WithMany(priceList => priceList.PriceItems)
            .HasForeignKey(item => item.PriceListId);

        modelBuilder.Entity<Customer>().HasData(
            new { Id = 1, Name = "Công ty Minh Phát", Status = "Active" },
            new { Id = 2, Name = "Công ty Đại Dương", Status = "Active" });

        modelBuilder.Entity<Contract>().HasData(new
        {
            Id = 1,
            ContractNo = "HD-2026-018",
            CustomerId = 1,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2027, 9, 30),
            Status = "Draft"
        });

        modelBuilder.Entity<PriceList>().HasData(new
        {
            Id = 1,
            Name = "Bảng giá dịch vụ 2026",
            EffectiveDate = new DateOnly(2026, 10, 1)
        });

        modelBuilder.Entity<PriceItem>().HasData(
            new { PriceListId = 1, ServiceCode = "SVC001", UnitPrice = 100000.00m },
            new { PriceListId = 1, ServiceCode = "SVC002", UnitPrice = 200000.00m });
    }
}
