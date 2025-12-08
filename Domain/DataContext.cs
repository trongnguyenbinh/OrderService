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

        modelBuilder.Entity<OrderEntity>().ToTable("orders").HasKey(x => x.Id);

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
    }

    public DbConnection GetDbConnection()
    {
        return Database.GetDbConnection();
    }

    public new DbSet<TEntity> Set<TEntity>() where TEntity : class
    {
        return base.Set<TEntity>();
    }

    public DbSet<OrderEntity> Orders {get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
}