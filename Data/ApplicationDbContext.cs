using JewelEase.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace JewelEase.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<JewelryItems> JewelryItems { get; set; }
        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<ImageSearchResults> ImageSearchResults { get; set; }
        public DbSet<InventoryHistory> InventoryHistory { get; set; }
        public DbSet<ItemQuotation> ItemQuotation { get; set; }
		public DbSet<ItemQuotationLineItem> ItemQuotationLineItem { get; set; }
        public DbSet<Slider> Slider { get; set; }
        public DbSet<Sales> Sales{ get; set; }
        public DbSet<SalesLineItem> SalesLineItem{ get; set; }
    }
}
