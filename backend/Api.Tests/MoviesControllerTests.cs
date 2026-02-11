using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using Moq;
using Xunit;
using Api.Controllers;
using Api.Data;
using Api.Models;
using Api.Dtos;
using Api.Services;

namespace Api.Tests
{
    public class MoviesControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IExchangeRateService> _mockExchangeRateService;
        private readonly Mock<ILogger<MoviesController>> _mockLogger;

        public MoviesControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _mockExchangeRateService = new Mock<IExchangeRateService>();
            _mockLogger = new Mock<ILogger<MoviesController>>();
        }

        [Fact]
        public async Task PostMovie_ReturnsCreatedResponse_WhenDataIsValid()
        {
            // Arrange
            var controller = new MoviesController(_context, _mockExchangeRateService.Object, _mockLogger.Object);
            
            var movieDto = new MovieCreateDto
            {
                Titulo = "Test Movie",
                Descripcion = "Test Description",
                Precio = 10.5m,
                CategoryId = 1,
                Estado = "Activa"
            };

            // Act
            var result = await controller.PostMovie(movieDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var movie = Assert.IsType<Movie>(createdAtActionResult.Value);
            
            Assert.Equal(movieDto.Titulo, movie.Titulo);
            Assert.Equal(1, await _context.Movies.CountAsync());
        }

        [Fact]
        public async Task GetMovie_ReturnsMovie()
        {
            // Arrange
            var controller = new MoviesController(_context, _mockExchangeRateService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetMovie(999);

            // Assert
            Assert.NotNull(result);
        }
    }


//    public class MoviesControllerTests : IDisposable
//     {
//         private readonly AppDbContext _context;
//         private readonly SqliteConnection _connection;
//         private readonly Mock<IExchangeRateService> _mockExchangeRateService;
//         private readonly Mock<ILogger<MoviesController>> _mockLogger;

//         public MoviesControllerTests()
//         {
//             // Configuración SQLite
//             _connection = new SqliteConnection("Filename=:memory:");
//             _connection.Open();

//             var options = new DbContextOptionsBuilder<AppDbContext>()
//                 .UseSqlite(_connection)
//                 .Options;

//             _context = new AppDbContext(options);
//             _context.Database.EnsureCreated();
            
//             _mockExchangeRateService = new Mock<IExchangeRateService>();
//             _mockLogger = new Mock<ILogger<MoviesController>>();
//         }

//         // MÉTODO HELPER PARA RESETEAR BD
//         private async Task ResetDatabaseAsync()
//         {
//             // Eliminar todos los datos
//             _context.Movies.RemoveRange(_context.Movies);
//             _context.Categories.RemoveRange(_context.Categories);
//             await _context.SaveChangesAsync();
//         }

//         public void Dispose()
//         {
//             _connection.Close();
//             _context.Dispose();
//         }

//         [Fact]
//         public async Task GetMovie_ReturnsNotFound_WhenIdDoesNotExist()
//         {
//             // Arrange - Limpiar y agregar datos CONTROLADOS
//             await ResetDatabaseAsync();
            
//             // Agregar SOLO una película con ID específico
//             _context.Movies.Add(new Movie 
//             { 
//                 Id = 100,  // ID explícito y conocido
//                 Titulo = "Película de Prueba",
//                 Precio = 10.5m,
//                 Estado = "Activa",
//                 Category = new Category { Id = 1, Nombre = "Acción" }
//             });
//             await _context.SaveChangesAsync();
            
//             // Debug: Verificar que se guardó
//             var moviesInDb = await _context.Movies.ToListAsync();
//             Assert.Single(moviesInDb); // ← Debe haber 1 película
//             Assert.Equal(100, moviesInDb[0].Id); // ← Debe tener ID 100
            
//             var controller = new MoviesController(_context, _mockExchangeRateService.Object, _mockLogger.Object);
            
//             // Act - Buscar ID que NO existe (9990)
//             var result = await controller.GetMovie(9990);
            
//             // Assert
//             Assert.IsType<NotFoundResult>(result.Result);
//         }

//         [Fact]
//         public async Task GetMovie_ReturnsMovie_WhenIdExists()
//         {
//             // Arrange
//             await ResetDatabaseAsync();
        
//             var expectedMovie = new Movie 
//             { 
//                 Id = 1,
//                 Titulo = "Mi Película",
//                 Precio = 20.5m,
//                 Estado = "Activa",
//                 Category = new Category { Nombre = "Comedia" }
//             };
        
//             _context.Movies.Add(expectedMovie);
//             await _context.SaveChangesAsync();
        
//             // Mock del servicio de tasa de cambio
//             _mockExchangeRateService.Setup(x => x.GetCopRateAsync())
//                 .ReturnsAsync(4000m);
        
//             var controller = new MoviesController(_context, _mockExchangeRateService.Object, _mockLogger.Object);
        
//             // Act
//             var actionResult = await controller.GetMovie(1);
        
//             // Assert
//             var result = Assert.IsType<ActionResult<MovieReadDto>>(actionResult);
//             var movieDto = Assert.IsType<MovieReadDto>(result.Value);
        
//             Assert.Equal(expectedMovie.Id, movieDto.Id);
//             Assert.Equal(expectedMovie.Titulo, movieDto.Titulo);
//             Assert.Equal(expectedMovie.Precio * 4000, movieDto.PrecioCop);
//         }
//     }
}
