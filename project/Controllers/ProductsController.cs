using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace JustBuyApi.Controllers;

// Имеет методы доступные только для администратора
/// <inheritdoc />
[ApiController]
[Route("/products")]
public class ProductsController : ControllerBase
{
    private readonly ProjectContext _context;

    /// <inheritdoc />
    public ProductsController(ProjectContext context)
    {
        _context = context;
    }

#pragma warning disable CS1591
    public class AdminProduct
    {
        [ValidateNever] public string? Name { get; set; }
        [ValidateNever] public string? Description { get; set; }
        [ValidateNever] public int Price { get; set; }
    }
#pragma warning restore CS1591
    
    // API: /products - получение списка всех товаров
    /// <summary>
    /// Возвращает массив всех товаров
    /// </summary>
    /// <returns>Массив товаров</returns>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(200)]
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
    /// <summary>
    /// Добавляет товар с переданным набором полей
    /// </summary>
    /// <param name="adminProduct">Необходимые поля для создания товара</param>
    /// <returns>Сообщение об успешном добавлении товара</returns>
    /// <response code="201">Успешное добавление товара</response>
    /// <response code="422">Один или несколько параметры были неверны.</response>
    [HttpPost]
    [Route("/product")]
    [Produces("application/json")]
    [Authorize(Roles = "1")]
    [ProducesResponseType(201)]
    [ProducesResponseType(typeof(ErrorsController.ErrorClass), 422)]
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
    /// <summary>
    /// Удаляет товар с заданным id
    /// </summary>
    /// <param name="id">Id товара в БД</param>
    /// <returns>Сообщение об успешном удалении</returns>
    /// <response code="200">Товар успешно удален</response>
    /// <response code="422">Данный товар не найден</response>
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
    /// <summary>
    /// Изменяет товар с заданным id
    /// </summary>
    /// <param name="adminProduct">Поля необходимые для изменения, необязательные</param>
    /// <param name="id">Id товара в БД</param>
    /// <returns>Измененный вариант товара</returns>
    /// <response code="200">Изменения внесены</response>
    /// <response code="422">Товар с заданным id не найден, или ошибка валидации изменений</response>
    [HttpPatch]
    [Route("/product/{id:int}")]
    [Produces("application/json")]
    [Authorize(Roles = "1")]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(ErrorsController.ErrorClass), 422)]
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