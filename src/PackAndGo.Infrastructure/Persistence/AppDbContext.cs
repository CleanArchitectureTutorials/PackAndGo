using Microsoft.EntityFrameworkCore;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
   public DbSet<UserDataModel> Users { get; set; }
    public DbSet<PackingListDataModel> PackingLists { get; set; }
    public DbSet<ItemDataModel> Items { get; set; }

   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<UserDataModel>().HasData(
           new UserDataModel { Id = Guid.NewGuid(), Email = "john.doe@test.com" },
           new UserDataModel { Id = Guid.NewGuid(), Email = "jane.doe@test.com" },
           new UserDataModel { Id = Guid.NewGuid(), Email = "jacob.doe@test.com" }
       );
   }
}