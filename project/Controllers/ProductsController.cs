using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace JustBuyApi.Controllers;

// Имеет методы доступные только для администратора
[ApiController]
[Route("/products")]
public class ProductsController : ControllerBase
{
    private readonly ProjectContext _context;

    public ProductsController(ProjectContext context)
    {
        _context = context;
    }

    public class AdminProduct
    {
        [ValidateNever] public string? Name { get; set; }
        [ValidateNever] public string? Description { get; set; }
        [ValidateNever] public int Price { get; set; }
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
    
    // API: /product - добавление товара
    [HttpPost]
    [Route("/product")]
    [Produces("application/json")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> AddNewProduct([FromBody] AdminProduct adminProduct)
    {
        // Словарь ошибок, где ключ - название поля, а значение - ошибка
        var errors = new Dictionary<string, string>();
        
        // Проверка названия товара на пустоту
        if (string.IsNullOrWhiteSpace(adminProduct.Name))
        {
            errors.Add("name", @"[ ""Name is required"" ]");
        }
        
        // Проверка описания товара на пустоту
        if (string.IsNullOrWhiteSpace(adminProduct.Description))
        {
            errors.Add("description", @"[ ""Description is required"" ]");
        }
        
        // Проверка цены товара на пустоту
        if (adminProduct.Price == 0)
        {
            errors.Add("price", @"[ ""Price is required"" ]");
        }
        else
        {
            // Проверка цены товара на отрицательность
            if (adminProduct.Price < 0)
            {
                errors.Add("price", @"[ ""Price must be greater than 0"" ]");
            }
        }
        
        // Если есть ошибки, то возвращаем их
        if (errors.Count > 0)
        {
            return new ContentResult
            {
                Content =
                    $@"{{""error"": {{ ""code"": 422, ""message"": ""Validation error"", ""errors"": {{ {string.Join(", ", errors.Select(x => $"\"{x.Key}\": {x.Value}"))} }} }} }}",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Создаем новый товар
        var product = new Product
        {
            Name = adminProduct.Name,
            Description = adminProduct.Description,
            Price = adminProduct.Price
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Возвращаем сообщение и id товара
        return new ContentResult
        {
            Content = $@"{{ ""data"": {{ ""id"": {product.Id}, ""message"": ""Product added"" }} }}",
            ContentType = "application/json",
            StatusCode = 201
        };
    }
    
    // API: /product/{id} - удаление товара, где id - идентификатор товара
    [HttpDelete]
    [Route("/product/{id:int}")]
    [Produces("application/json")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // Получаем товар по идентификатору
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        
        // Если товар не найден, то возвращаем ошибку
        if (product == null)
        {
            return new ContentResult
            {
                Content = $@"{{""error"": {{ ""code"": 422, ""message"": ""Validation error"", ""errors"": {{ ""id"": [ ""Product not found"" ] }} }} }}",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Удаляем товар
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // Возвращаем сообщение об успешном удалении товара
        return new ContentResult
        {
            Content = $@"{{ ""data"": {{ ""message"": ""Product removed"" }} }}",
            ContentType = "application/json",
            StatusCode = 200
        };
    }
    
    // API: /product/{id} - изменение товара, где id - идентификатор товара
    [HttpPatch]
    [Route("/product/{id:int}")]
    [Produces("application/json")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> PatchProduct([FromBody] AdminProduct adminProduct, int id)
    {
        // Получаем товар по идентификатору
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        
        // Если товар не найден, то возвращаем ошибку
        if (product == null)
        {
            return new ContentResult
            {
                Content = @"{{""error"": {{ ""code"": 422, ""message"": ""Validation error"", ""errors"": {{ ""id"": [ ""Product not found"" ] }} }} }}",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Словарь ошибок, где ключ - название поля, а значение - ошибка
        var errors = new Dictionary<string, string>();
        
        // Проверка названия товара на пустоту
        if (string.IsNullOrWhiteSpace(adminProduct.Name))
        {
            adminProduct.Name = product.Name;
        }
        
        // Проверка описания товара на пустоту
        if (string.IsNullOrWhiteSpace(adminProduct.Description))
        {
            adminProduct.Description = product.Description;
        }
        
        // Проверка цены товара на пустоту
        if (adminProduct.Price == 0)
        {
            adminProduct.Price = product.Price;
        }
        else
        {
            // Проверка цены товара на отрицательность
            if (adminProduct.Price < 0)
            {
                errors.Add("price", @"[ ""Price must be greater than 0"" ]");
            }
        }
        
        // Если есть ошибки, то возвращаем их
        if (errors.Count > 0)
        {
            return new ContentResult
            {
                Content =
                    $@"{{""error"": {{ ""code"": 422, ""message"": ""Validation error"", ""errors"": {{ {string.Join(", ", errors.Select(x => $"\"{x.Key}\": {x.Value}"))} }} }} }}",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Изменяем товар
        product.Name = adminProduct.Name;
        product.Description = adminProduct.Description;
        product.Price = adminProduct.Price;
        await _context.SaveChangesAsync();

        // Возвращаем сообщение об успешном изменении товара
        return new ContentResult
        {
            Content = $@"{{ ""data"": {{ ""id"": ""{product.Id}"", ""name"": ""{product.Name}"", ""description"": ""{product.Description}"", ""price"": ""{product.Price}"" }} }}",
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}