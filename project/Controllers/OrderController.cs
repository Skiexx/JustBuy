using JustBuyApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustBuyApi.Controllers;

// Доступен только авторизованным пользователям [Authorize(Roles = 2)]
// 2 - это роль клиента
[ApiController]
[Route("/order")]
public class OrderController : ControllerBase
{
    private readonly ProjectContext _context;

    public OrderController(ProjectContext context)
    {
        _context = context;
    }
    
    // API: /order - оформление заказа
    [HttpPost]
    [Authorize(Roles = "2")]
    [Produces("application/json")]
    public async Task<IActionResult> PostOrder()
    {
        // Получение id пользователя из токена
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value, out var userId);
        
        // Поиск заказа в БД
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        if (order == null)
        {
            // Если заказа нет, то возвращаем ошибку
            return new ContentResult
            {
                Content = @"{ ""error"": { ""code"": 422, ""message"": ""Cart is empty"" } }",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Поиск товаров в заказе
        var cart = await _context.Carts.Where(c => c.OrderId == order.Id).ToListAsync();
        if (!cart.Any())
        {
            // Если товаров нет, то возвращаем ошибку
            return new ContentResult
            {
                Content = @"{ ""error"": { ""code"": 422, ""message"": ""Cart is empty"" } }",
                ContentType = "application/json",
                StatusCode = 422
            };
        }
        
        // Если заказ есть, то меняем статус на оплаченный
        order.Payed = true;
        await _context.SaveChangesAsync();
        
        // Возвращаем успешный ответ
        return new ContentResult
        {
            Content = @"{ ""success"": { ""code"": 200, ""message"": ""Order payed"" } }",
            ContentType = "application/json",
            StatusCode = 200
        };
    }
    
    // API: /order - просмотр оформленных заказов
    [HttpGet]
    [Authorize(Roles = "2")]
    [Produces("application/json")]
    public async Task<IActionResult> GetOrders()
    {
        // Получение id пользователя из токена
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value, out var userId);
        
        // Поиск заказов в БД
        var orders = await _context.Orders.Where(o => o.UserId == userId && o.Payed == true).ToListAsync();
        if (!orders.Any())
        {
            // Если заказов нет, то возвращаем пустой массив
            return new ContentResult
            {
                Content = @"{ ""data"": [] }",
                ContentType = "application/json",
                StatusCode = 200
            };
        }
        
        // Формирование ответа
        var ordersResponse = new List<string>();
        for (int i = 0; i < orders.Count; i++)
        {
            // Поиск товаров в заказе
            var cartItems = await _context.Carts.Where(c => c.OrderId == orders[i].Id).ToListAsync();
            // Добавление блока с информацией о заказе
            ordersResponse.Add(
                @$"{{ ""id"": {orders[i].Id}, ""products"": [{string.Join(",", cartItems.Select(c => c.ProductId))}], ""order_price"": {cartItems.Sum(c => c.Product!.Price * c.Quantity)} }}");
        }
        var response = @$"{{ ""data"": [{string.Join(", ", ordersResponse)}] }}";
        
        // Возвращаем ответ
        return new ContentResult
        {
            Content = response,
            ContentType = "application/json",
            StatusCode = 200
        };
    }
}