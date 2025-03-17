using Microsoft.EntityFrameworkCore;
using SidoAgung.ApiSaga.Infrastruktur.Models;

namespace SidoAgung.ApiSaga.Infrastruktur.Persistences;
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<CustomerModel> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

