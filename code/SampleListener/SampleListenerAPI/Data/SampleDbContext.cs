using Microsoft.EntityFrameworkCore;
using SimpleTools.SimpleHooks.SampleListener.SampleListenerAPI.Models;

namespace SimpleTools.SimpleHooks.SampleListener.SampleListenerAPI.Data
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
