using JustBuyApi.Models;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS1591

namespace JustBuyApi.Data;

public class ProjectContext : DbContext
{
    public ProjectContext (DbContextOptions<ProjectContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(r =>
        {
            r.HasData(new Role
                {
                    Id = 1,
                    Title = "Администратор"
                },
                new Role
                {
                    Id = 2,
                    Title = "Клиент"
                }
            );
        });
            
        modelBuilder.Entity<User>(u =>
        {
            u.HasIndex(us => us.Email).IsUnique();
            u.HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@shop.ru",
                    FullName = "Администратор",
                    // ReSharper disable once StringLiteralTypo
                    Password = "QWEasd123",
                    RoleId = 1
                },
                new User
                {
                    Id = 2,
                    Email = "user@shop.ru",
                    FullName = "Клиент",
                    Password = "password",
                    RoleId = 2
                }
            );
        });

        modelBuilder.Entity<Product>(p =>
        {
            p.HasData(
                new Product
                {
                    Id = 1,
                    Name = "Кофе",
                    Description = "Кофе",
                    Price = 100,
                },
                new Product
                {
                    Id = 2,
                    Name = "Чай",
                    Description = "Чай",
                    Price = 50,
                },
                new Product
                {
                    Id = 3,
                    Name = "Сок",
                    Description = "Сок",
                    Price = 150,
                }
            );
        });

        modelBuilder.Entity<Order>(o =>
        {
            o.HasData(
                new Order
                {
                    Id = 1,
                    UserId = 2,
                    Payed = true
                },
                new Order
                {
                    Id = 2,
                    UserId = 2,
                    Payed = false
                }
            );
        });

        modelBuilder.Entity<Cart>(c =>
        {
            c.HasData(
                new Cart
                {
                    Id = 1,
                    OrderId = 1,
                    ProductId = 1,
                    Quantity = 1
                },
                new Cart
                {
                    Id = 2,
                    OrderId = 1,
                    ProductId = 2,
                    Quantity = 2
                },
                new Cart
                {
                    Id = 3,
                    OrderId = 2,
                    ProductId = 3,
                    Quantity = 1
                }
            );
        });
    }

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Cart> Carts { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
}