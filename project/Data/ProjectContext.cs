using JustBuyApi.Models;
using Microsoft.EntityFrameworkCore;

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
                });
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
                });
        });
    }

    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Cart> Carts { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
}