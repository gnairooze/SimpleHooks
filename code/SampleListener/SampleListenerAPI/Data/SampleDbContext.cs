using Microsoft.EntityFrameworkCore;
using SampleListenerAPI.Models;

namespace SampleListenerAPI.Data
{
    
    public class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
    {
        public DbSet<SampleModel> SampleModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SampleModel>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
