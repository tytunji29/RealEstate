using System.Collections.Generic;
using System.Reflection.Emit;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace RealEstate.Models;

public class RealEstateDbContext : IdentityDbContext<ApplicationUser>
{
    public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options) : base(options) { }
    public DbSet<Property> Properties { get; set; }
    public DbSet<SellerInfo> SellerInfo { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<PropertyViewers> PropertyViewers { get; set; }
    public DbSet<UnverifiedSellerProduct> UnverifiedSellerProducts { get; set; }
    public DbSet<EscrowAccount> EscrowAccounts { get; set; }
    public DbSet<EscrowTransaction> EscrowTransactions { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Property>().HasOne(p => p.Seller).WithMany(u => u.Properties).HasForeignKey(p => p.SellerId).OnDelete(DeleteBehavior.Restrict); builder.Entity<PropertyImage>().HasOne(pi => pi.Property).WithMany(p => p.Images).HasForeignKey(pi => pi.PropertyId); builder.Entity<PropertyViewers>().HasOne(pv => pv.Property).WithMany(p => p.PropertyViewers).HasForeignKey(pv => pv.PropertyId); builder.Entity<PropertyViewers>().HasOne(pv => pv.Viewer).WithMany(u => u.PropertyViewers).HasForeignKey(pv => pv.ViewerId).OnDelete(DeleteBehavior.Restrict); builder.Entity<EscrowAccount>().HasOne(ea => ea.User).WithOne().HasForeignKey<EscrowAccount>(ea => ea.UserId); builder.Entity<EscrowTransaction>().HasOne(et => et.EscrowAccount).WithMany(ea => ea.Transactions).HasForeignKey(et => et.EscrowAccountId);
    }
}
