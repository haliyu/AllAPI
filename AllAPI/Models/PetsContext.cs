using Microsoft.EntityFrameworkCore;
using System;


namespace AllAPI.Controllers
{
    public class PetsContext : DbContext
    {
        public PetsContext(DbContextOptions<PetsContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
