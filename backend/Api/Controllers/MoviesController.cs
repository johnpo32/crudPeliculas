using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Dtos;
using Api.Services;
using Api.Filters;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiKey]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(AppDbContext context, IExchangeRateService exchangeRateService, ILogger<MoviesController> logger)
        {
            _context = context;
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieReadDto>>> GetMovies()
        {
            var rate = await _exchangeRateService.GetCopRateAsync();
            var movies = await _context.Movies.Include(m => m.Category).ToListAsync();
            
            return movies.Select(m => new MovieReadDto
            {
                Id = m.Id,
                Titulo = m.Titulo,
                Descripcion = m.Descripcion,
                PrecioUsd = m.Precio,
                PrecioCop = m.Precio * rate,
                CategoryId = m.CategoryId,
                CategoryNombre = m.Category?.Nombre ?? "N/A",
                Estado = m.Estado
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieReadDto>> GetMovie(int id)
        {
            var movie = await _context.Movies.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();

            var rate = await _exchangeRateService.GetCopRateAsync();
            return new MovieReadDto
            {
                Id = movie.Id,
                Titulo = movie.Titulo,
                Descripcion = movie.Descripcion,
                PrecioUsd = movie.Precio,
                PrecioCop = movie.Precio * rate,
                CategoryId = movie.CategoryId,
                CategoryNombre = movie.Category?.Nombre ?? "N/A",
                Estado = movie.Estado
            };
        }

        [HttpPost]
        public async Task<ActionResult<MovieReadDto>> PostMovie(MovieCreateDto movieDto)
        {
            var movie = new Movie
            {
                Titulo = movieDto.Titulo,
                Descripcion = movieDto.Descripcion,
                Precio = movieDto.Precio,
                CategoryId = movieDto.CategoryId,
                Estado = movieDto.Estado
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nueva película registrada: {Titulo} (ID: {Id}) | Precio: {Precio} | Categoría: {CategoriaId}", 
                movie.Titulo, movie.Id, movie.Precio, movie.CategoryId);

            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(int id, MovieUpdateDto movieDto)
        {
            if (id != movieDto.Id) return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.Titulo = movieDto.Titulo;
            movie.Descripcion = movieDto.Descripcion;
            movie.Precio = movieDto.Precio;
            movie.CategoryId = movieDto.CategoryId;
            movie.Estado = movieDto.Estado;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Película actualizada: {Titulo} (ID: {Id}) | Precio: {Precio} | Categoría: {CategoriaId}", 
                movie.Titulo, movie.Id, movie.Precio, movie.CategoryId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var titulo = movie.Titulo;
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Película eliminada: {Titulo} (ID: {Id})", titulo, id);

            return NoContent();
        }
    }
}
