using System.Data.Entity;
using Reinforced.SqlStroke.Demo.Data;

namespace Reinforced.SqlStroke.Demo
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("mydata")
        {
        }


        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<Item> Items { get; set; }

       
        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Item>().ToTable("Goods");

            builder.Entity<Customer>().Property(x => x.RegisterDate).HasColumnName("RegisteredAt");

            builder.Entity<Order>().Property(x => x.Subtotal).HasColumnName("Total");
        }
    }
}
