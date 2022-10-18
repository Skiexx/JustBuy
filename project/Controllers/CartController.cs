using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustBuyApi.Controllers;

// Доступен только авторизованным пользователям [Authorize(Roles = 2)]
// 2 - это роль клиента
[ApiController]
[Route("/cart")]
public class CartController : ControllerBase
{
    private readonly ProjectContext _context;

    public CartController(ProjectContext context)
    {
        _context = context;
    }

    // API: /cart/{product_id} - Добавление товара в корзину, где product_id - id товара
    [HttpPost("{product_id:int}")]
    [Authorize(Roles = "2")]
    [Produces("application/json")]
    public async Task<IActionResult> AddToCart(int product_id)
    {
        // Поиск товара в базе данных
        var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == product_id);
        if (product == null)
        {
            // Если товар не найден, возвращаем ошибку
            return new ContentResult
            {
                Content = @"{ ""error"": { ""code"": 422, ""message"": ""Validation error"", ""errors"": { ""product"": [""Product not found""] } } }",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Получение id пользователя из токена
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value, out var userId);
        
        // Поиск открытого заказа в БД
        var orderId = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        if (orderId == null)
        {
            // Если открытый заказ не найден, создаем новый
            var order = new Order
            {
                UserId = userId,
                Payed = false,
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            orderId = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        }
        
        // Поиск товара в корзине
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.OrderId == orderId!.Id && c.ProductId == product_id);
        if (cart == null)
        {
            // Если товар не найден в корзине, добавляем его
            cart = new Cart
            {
                OrderId = orderId!.Id,
                Quantity = 1,
                Product = product
            };
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Если товар найден в корзине, увеличиваем количество
            cart.Quantity++;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
        
        // Возвращаем успешный ответ
        return new ContentResult
        {
            Content = @"{ ""data"": { ""message"": ""Product added to cart"" } }",
            ContentType = "application/json",
            StatusCode = 201
        };
    }
    
    // API: /cart - Получение списка товаров в корзине
    [HttpGet]
    [Authorize(Roles = "2")]
    [Produces("application/json")]
    public async Task<IActionResult> GetCart()
    {
        // Получение id пользователя из токена
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value, out var userId);
        
        // Поиск открытого заказа в БД
        var orderId = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        if (orderId == null)
        {
            // Если открытый заказ не найден, возвращаем пустой список
            return new ContentResult
            {
                Content = @"{ ""data"": [] }",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        
        // Получение списка товаров в корзине
        var cart = await _context.Carts.Where(c => c.OrderId == orderId!.Id).ToListAsync();
        if (!cart.Any())
        {
            // Если товары в корзине не найдены, возвращаем пустой список
            return new ContentResult
            {
                Content = @"{ ""data"": [] }",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        // Получение списка товаров из БД
        var products = await _context.Products.ToListAsync();
        
        // Формирование ответа
        var cartItemsResponse = new List<string>();
        for (int i = 0; i < cart.Count; i++)
        {
            // Поиск товара в БД
            var product = products.FirstOrDefault(p => p.Id == cart[i].ProductId)!;
            // Добавление блока с информацией о товаре в ответ
            cartItemsResponse.Add(
                $@"{{ ""id"": {cart[i].Id}, ""quantity"": ""{cart[i].Quantity}"", ""product_id"": ""{cart[i].ProductId}"", ""name"": ""{product.Name}"", ""description"": ""{product.Description}"", ""price"": ""{product.Price}"" }}");
        }
        var response = $@"{{ ""data"": [ {string.Join(", ", cartItemsResponse)} ] }}";
        
        // Возвращаем ответ
        return new ContentResult
        {
            Content = response,
            ContentType = "application/json",
            StatusCode = 200
        };
    }
    
    // API: /cart/{id} - Удаление товара из корзины, где id - id товара в корзине
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "2")]
    [Produces("application/json")]
    public async Task<IActionResult> DeleteFromCart(int id)
    {
        // Полуение id пользователя из токена
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value, out var userId);
        
        // Поиск открытого заказа в БД
        var orderId = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        if (orderId == null)
        {
            // Если открытый заказ не найден, возвращаем ошибку
            return new ContentResult
            {
                Content = @"{ ""error"": { ""code"": 422, ""message"": ""Validation error"", ""errors"": { ""id"": ""Order not found"" } } }",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Поиск товара в корзине
        var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.OrderId == orderId.Id);
        // Если товар не найден, возвращаем ошибку
        if (cartItem == null)
        {
            // Возвращаем ошибку
            return new ContentResult
            {
                Content = @"{ ""error"": { ""code"": 422, ""message"": ""Validation error"", ""errors"": { ""id"": ""Item in cart not found"" } } }",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Удаление товара из корзины
        _context.Carts.Remove(cartItem);
        await _context.SaveChangesAsync();
        
        // Возвращаем ответ
        return new ContentResult
        {
            Content = @"{ ""data"": { ""message"": ""Item removed from cart"" } }",
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}