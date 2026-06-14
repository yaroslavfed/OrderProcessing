using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity
                    .Property(x => x.UserName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity
                    .Property(x => x.ProductName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity
                    .Property(x => x.Quantity)
                    .IsRequired();

                entity
                    .Property(x => x.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                entity
                    .Property(x => x.CreatedAt)
                    .IsRequired();
            }
        );
    }
}