using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Dtos;
using Api.Filters;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiKey]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            return await _context.Categories
                .Select(c => new CategoryDto { Id = c.Id, Nombre = c.Nombre, Descripcion = c.Descripcion })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return new CategoryDto { Id = category.Id, Nombre = category.Nombre, Descripcion = category.Descripcion };
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> PostCategory(CategoryDto categoryDto)
        {
            var category = new Category { Nombre = categoryDto.Nombre, Descripcion = categoryDto.Descripcion };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Nueva categoría registrada: {Nombre} (ID: {Id})", category.Nombre, category.Id);

            categoryDto.Id = category.Id;
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryDto categoryDto)
        {
            if (id != categoryDto.Id) return BadRequest();
            
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Nombre = categoryDto.Nombre;
            category.Descripcion = categoryDto.Descripcion;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoría actualizada: {Nombre} (ID: {Id})", category.Nombre, category.Id);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            
            var nombre = category.Nombre;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoría eliminada: {Nombre} (ID: {Id})", nombre, id);

            return NoContent();
        }
    }
}
