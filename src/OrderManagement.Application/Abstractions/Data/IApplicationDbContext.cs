using Microsoft.EntityFrameworkCore;
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

namespace OrderManagement.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Brand> Brands{ get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<Order> Orders { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<ReturnOrder> ReturnOrders { get; }
    DbSet<Voucher> Vouchers { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Warehouse> Warehouses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

