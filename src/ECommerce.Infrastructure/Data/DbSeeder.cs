using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await context.Database.MigrateAsync();

        // Seed Roles
        string[] roles = ["Admin", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Admin User
        const string adminEmail = "admin@ecommerce.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var electronics = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Electronics",
                Slug = "electronics"
            };

            var clothing = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Clothing",
                Slug = "clothing"
            };

            var books = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Books",
                Slug = "books"
            };

            context.Categories.AddRange(electronics, clothing, books);
            await context.SaveChangesAsync();

            // Seed Products
            if (!await context.Products.AnyAsync())
            {
                var products = new List<Product>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Wireless Bluetooth Headphones",
                        Description = "Premium noise-cancelling wireless headphones with 30-hour battery life and comfortable over-ear design.",
                        Price = 79.99m,
                        Stock = 150,
                        ImageUrl = "https://full-featured-ecommerce-aspnet.runasp.net/images/headphones.jpg",
                        CategoryId = electronics.Id,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "4K Ultra HD Smart TV 55\"",
                        Description = "55-inch 4K UHD Smart TV with HDR support, built-in streaming apps, and voice control.",
                        Price = 549.99m,
                        Stock = 40,
                        ImageUrl = "https://full-featured-ecommerce-aspnet.runasp.net/images/smart-tv.jpg",
                        CategoryId = electronics.Id,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Classic Cotton T-Shirt",
                        Description = "Soft 100% organic cotton t-shirt available in multiple colors. Perfect for everyday wear.",
                        Price = 24.99m,
                        Stock = 500,
                        ImageUrl = "https://full-featured-ecommerce-aspnet.runasp.net/images/tshirt.jpg",
                        CategoryId = clothing.Id,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Slim Fit Denim Jeans",
                        Description = "Modern slim fit jeans made from stretch denim for all-day comfort and style.",
                        Price = 59.99m,
                        Stock = 200,
                        ImageUrl = "https://full-featured-ecommerce-aspnet.runasp.net/images/jeans.jpg",
                        CategoryId = clothing.Id,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "The Art of Clean Code",
                        Description = "A comprehensive guide to writing maintainable, readable, and efficient code for modern developers.",
                        Price = 34.99m,
                        Stock = 300,
                        ImageUrl = "https://full-featured-ecommerce-aspnet.runasp.net/images/clean-code.jpg",
                        CategoryId = books.Id,
                        IsActive = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Mastering Cloud Architecture",
                        Description = "In-depth exploration of cloud design patterns, scalability strategies, and best practices.",
                        Price = 44.99m,
                        Stock = 250,
                        ImageUrl = "https://full-featured-ecommerce-aspnet.runasp.net/images/cloud-architecture.jpg",
                        CategoryId = books.Id,
                        IsActive = true
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
