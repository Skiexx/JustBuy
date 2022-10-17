using JustBuyApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustBuyApi.Controllers;

[ApiController]
[Route("/products")]
public class ProductsController : ControllerBase
{
    private readonly ProjectContext _context;

    public ProductsController(ProjectContext context)
    {
        _context = context;
    }
    
    // API: /products - получение списка всех товаров
    [HttpGet]
    [Produces("application/json")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        
        // Проверка на пустой список
        if (products.Count == 0)
        {
            // Возвращаем пустой список
            return new ContentResult
            {
                Content = @"{ ""data"": [] }",
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        // Возвращаем список товаров
        return new ContentResult
        {
            Content =
                $@"{{ ""data"": [{string.Join(", ", products.Select(p => $@"{{""{nameof(p.Id).ToLower()}"": {p.Id}, ""{nameof(p.Name).ToLower()}"": ""{p.Name}"", ""{nameof(p.Description).ToLower()}"": ""{p.Description}"", ""{nameof(p.Price).ToLower()}"": ""{p.Price}""}}"))}] }}",
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}