using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions.Data;
using OrderManagement.Domain.Brands;
using OrderManagement.Domain.Categories;
using OrderManagement.Domain.Customers;
using OrderManagement.Domain.Inventories;
using OrderManagement.Domain.orders;
using OrderManagement.Domain.Products;
using OrderManagement.Domain.Promotions;
using OrderManagement.Domain.ReturnOrders;
using OrderManagement.Domain.Vouchers;
using OrderManagement.Domain.Warehouses;
using OrderManagement.Infrastructure.outbox;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Database;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Brand> Brands { get; }
    public DbSet<Category> Categories { get; }
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Inventory> Inventories { get; }
    public DbSet<Order> Orders { get; }
    public DbSet<Promotion> Promotions { get; }
    public DbSet<ReturnOrder> ReturnOrders { get; }
    public DbSet<Voucher> Vouchers { get; }
    public DbSet<Customer> Customers { get; }
    public DbSet<Warehouse> Warehouses { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);
        base.OnModelCreating(modelBuilder);
    }
}

