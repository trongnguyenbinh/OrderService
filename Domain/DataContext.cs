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
        modelBuilder.Entity<ProductEntity>().ToTable("products").HasKey(x => x.Id);
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
}