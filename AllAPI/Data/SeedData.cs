using AllAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllAPI
{
    public static class SeedData
    {
        public static void Initialize(PetsContext context)
        {
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Id=1,
                        Name = "HalisCode",
                        Price = 15.9m
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Hassan",
                        Price = 37.4m
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
