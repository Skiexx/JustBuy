using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS1591

namespace JustBuyApi.Controllers;

[ApiController]
[AllowAnonymous]
public abstract class ErrorsController : ControllerBase
{
    public abstract class ErrorClass
    {
        public ErrorDetails? Error { get; set; }
        
        public abstract class ErrorDetails
        {
            public int Code { get; set; }
            public string? Message { get; set; }
            public ErrorsCollection? Errors { get; set; }
            
            public abstract class ErrorsCollection
            {
                public List<string>? Key { get; set; }
                public List<string>? Key2 { get; set; }
                public List<string>? Key3 { get; set; }
            }
        }
    }

    
    
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/error")]
    public IActionResult Error()
    {
        return new ContentResult
        {
            Content = @"{ ""error"": { ""code"": 404, ""message"": ""Not found"" } }",
            ContentType = "application/json",
            StatusCode = 404
        };
    }
}
