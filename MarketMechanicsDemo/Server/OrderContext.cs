using MarketMechanicsDemo.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketMechanicsDemo.Server
{
    public class OrderContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=OrderBook.db");
    }
}
