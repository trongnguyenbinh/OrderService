using System.Data.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SKU).IsUnique();
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.SKU).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.Category).HasMaxLength(100);
        });

        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
            entity.Property(x => x.CustomerType).IsRequired();
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(x => x.SubTotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.OrderStatus).IsRequired();

            // Relationship with Customer
            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship with OrderItems
            entity.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);

            // Relationship with Product
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatSessionEntity>(entity =>
        {
            entity.ToTable("chat_sessions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserFingerprint);
            entity.Property(x => x.UserFingerprint).IsRequired().HasMaxLength(255);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.LastActivityAt).IsRequired();
            entity.Property(x => x.IsActive).IsRequired();

            // Relationship with ChatMessages
            entity.HasMany(x => x.Messages)
                .WithOne(x => x.Session)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessageEntity>(entity =>
        {
            entity.ToTable("chat_messages");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SessionId);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(20);
            entity.Property(x => x.Content).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.ModelUsed).HasMaxLength(50);
            entity.Property(x => x.ToolCalled).HasMaxLength(100);
        });
    }

    public DbConnection GetDbConnection()
    {
        return Database.GetDbConnection();
    }

    public new DbSet<TEntity> Set<TEntity>() where TEntity : class
    {
        return base.Set<TEntity>();
    }

    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<ChatSessionEntity> ChatSessions { get; set; }
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }
}