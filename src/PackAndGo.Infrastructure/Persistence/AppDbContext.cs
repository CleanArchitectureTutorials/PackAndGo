using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
   public DbSet<UserDataModel> DomainUsers { get; set; }
    public DbSet<PackingListDataModel> PackingLists { get; set; }
    public DbSet<ItemDataModel> Items { get; set; }

   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
        base.OnModelCreating(modelBuilder); // This is critical for configuring Identity tables
        
    //    modelBuilder.Entity<UserDataModel>().HasData(
    //        new UserDataModel { Id = Guid.NewGuid(), Email = "john.doe@test.com" },
    //        new UserDataModel { Id = Guid.NewGuid(), Email = "jane.doe@test.com" },
    //        new UserDataModel { Id = Guid.NewGuid(), Email = "jacob.doe@test.com" }
    //    );
   }
}