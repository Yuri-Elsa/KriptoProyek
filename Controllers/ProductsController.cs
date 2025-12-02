using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KriptoProyek.Models;
using KriptoProyek.Data;

namespace KriptoProyek.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET ALL - Publik, tidak perlu login
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        // Proteksi SQL Injection: Menggunakan EF Core (parameterized queries otomatis)
        var products = await _context.Products.ToListAsync();
        
        return Ok(products);
    }

    // GET BY ID - Publik
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            return NotFound(new { message = "Produk tidak ditemukan" });
        }
        
        return Ok(product);
    }

    // CREATE - Hanya Admin yang bisa
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // XSS Protection: ASP.NET Core otomatis encode output
        // Input validation dilakukan melalui Data Annotations
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // UPDATE - Hanya Admin yang bisa
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(new { message = "ID tidak cocok" });
        }

        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            return NotFound(new { message = "Produk tidak ditemukan" });
        }

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;

        await _context.SaveChangesAsync();

        return Ok(existingProduct);
    }

    // DELETE - Hanya Admin yang bisa
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            return NotFound(new { message = "Produk tidak ditemukan" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Produk berhasil dihapus" });
    }

    // SEARCH - Contoh query aman dari SQL Injection
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return BadRequest(new { message = "Keyword tidak boleh kosong" });
        }

        // EF Core otomatis parameterize query, aman dari SQL Injection
        var products = await _context.Products
            .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
            .ToListAsync();

        return Ok(products);
    }
}