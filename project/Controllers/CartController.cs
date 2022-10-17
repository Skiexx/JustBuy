using JustBuyApi.Data;
using JustBuyApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JustBuyApi.Controllers;
//TODO: Добавить коментарии
[ApiController]
[Route("/cart")]
public class CartController : ControllerBase
{
    private readonly ProjectContext _context;

    public CartController(ProjectContext context)
    {
        _context = context;
    }

    [HttpPost("{product_id:int}")]
    [Authorize(Roles = "2")]
    public async Task<IActionResult> AddToCart(int product_id)
    {
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "Id")!.Value, out var userId);
        var orderId = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        if (orderId == null)
        {
            var order = new Order
            {
                UserId = userId,
                Payed = false,
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            orderId = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Payed == false);
        }
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.OrderId == orderId!.Id && c.ProductId == product_id);
        if (cart == null)
        {
            cart = new Cart
            {
                OrderId = orderId!.Id,
                //TODO: Проверка товара на существование
                ProductId = product_id,
                Quantity = 1,
            };
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }
        else
        {
            cart.Quantity++;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }
        
        return new ContentResult
        {
            Content = @"{ ""data"": { ""message"": ""Product added to cart"" } }",
        };
    }
}