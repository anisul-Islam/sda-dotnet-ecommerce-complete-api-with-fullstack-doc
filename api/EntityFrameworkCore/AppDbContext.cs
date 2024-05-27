using api.EntityFrameworkCore;
using api.Models;
using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId); // Primary Key configuration
            entity.Property(u => u.UserId).HasDefaultValueSql("uuid_generate_v4()"); // generate UUID for new records
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.Address).HasMaxLength(255);
            entity.Property(u => u.IsAdmin).HasDefaultValue(false);
            entity.Property(u => u.IsBanned).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });


        // modelBuilder.Entity<Order>(entity =>
        // {
        //     entity.HasKey(o => o.UserId); // Primary Key
        //     entity.Property(o => o.OrderId).HasDefaultValueSql("uuid_generate_v4()");

        //     entity.Property(o => o.Status)
        //      .HasConversion<string>()  // Store enum as string in the database
        //      .HasDefaultValue(OrderStatus.Pending);  // Set default value

        //     entity.Property(o => o.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        //     // Configure the one-to-many relationship with Product
        //     entity.HasMany(o => o.Products)
        //           .WithOne(p => p.Order)
        //           .HasForeignKey(p => p.OrderId);
        // });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.ProductId); // Primary Key
            entity.Property(p => p.ProductId).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Slug).IsRequired();
            entity.Property(p => p.Price).IsRequired();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // Configure the many-to-many relationship with Category
            entity.HasMany(p => p.Categories)
                  .WithMany(c => c.Products);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryId).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // modelBuilder.Entity<User>(entity =>
        // {
        //     // Configure the one-to-many relationship with Order
        //     entity.HasMany(u => u.Orders)
        //           .WithOne(o => o.User)
        //           .HasForeignKey(o => o.UserId);
        // });
    }
}
