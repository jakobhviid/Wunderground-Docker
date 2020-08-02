using Microsoft.EntityFrameworkCore;
using Server.Data.Models;

namespace Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) {}
        public DbSet<Subscription> Subscriptions { get; set; }
    }
}