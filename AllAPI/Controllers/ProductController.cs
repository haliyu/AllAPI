using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly PetsContext _context;

        public ProductController(PetsContext context)
        {
            _context = context;
        }

        // Get All Data
        [HttpGet]
        public ActionResult<List<Product>> GetAll() =>
            _context.Products.ToList();

        // Get by Id


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if(product == null)
            {
                return NotFound();
            }

            return product;
        }

        // Create Data
        [HttpPost]
        public async Task<ActionResult<Product>> Post(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            

            return product;
        }

        // Delete Data
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();




            return null;
        }

        // Update Data
        [HttpPatch]
        public async Task<ActionResult<Product>> Patch( Product newduct)
        {

            var product = await _context.Products.FindAsync(newduct.Id);

           // _context.Products.Remove(product);
            
            product.Name = newduct.Name;
            product.Price = newduct.Price;

            //_context.Products.Update(product1);
            await _context.SaveChangesAsync();



            return product;
        }

    }
}
