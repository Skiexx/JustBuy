using JustBuyApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProjectContext>(options =>
    options
        .UseLazyLoadingProxies()
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOption.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOption.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOption.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Перехват ошибки авторизации и возврат сообщения вместо 401
                context.HandleResponse();
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                const string error = @"{ ""error"": { ""code"": 403, ""message"": ""Login failed"" } }";
                return context.Response.WriteAsync(error);
            },
            OnForbidden = context =>
            {
                // Перехват ошибки доступа и возврат сообщения вместо 403
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                const string error = @"{ ""error"": { ""code"": 403, ""message"": ""Forbidden for you"" } }";
                return context.Response.WriteAsync(error);
            }
        };
    });

builder.Services.AddControllers();
//builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
    {
    In = ParameterLocation.Header, 
    Description = "Please insert JWT with Bearer into field",
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { 
            new OpenApiSecurityScheme 
            { 
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" 
                } 
            },
            new string[] { } 
        } 
    });
    
    var filePath = Path.Combine(AppContext.BaseDirectory, "project.xml");
    c.IncludeXmlComments(filePath);
});

var app = builder.Build();

//app.UseProblemDetails();

app.UseSwagger(); 
app.UseSwaggerUI(options =>
{
    options.DefaultModelsExpandDepth(-1);
});

app.UseHttpsRedirection();
app.UseHsts();

app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithReExecute("/error");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ProjectContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.MapMethods("/login", new[] { "GET", "PATCH", "DELETE" }, async context => ReturnNotFound(context));
app.MapMethods("/signup", new[] {"GET", "PATCH", "DELETE"}, async context => ReturnNotFound(context));
app.MapMethods("/logout", new[] {"POST", "PATCH", "DELETE"}, async context => ReturnNotFound(context));
app.MapMethods("/cart", new[] {"POST", "DELETE", "PATCH"}, async context => ReturnNotFound(context));
app.MapMethods("/cart/{id}", new[] {"GET", "PATCH"}, async context => ReturnNotFound(context));
app.MapMethods("/order", new[] {"PATCH", "DELETE"}, async context => ReturnNotFound(context));
app.MapMethods("/products", new[] {"POST", "PATCH", "DELETE"}, async context => ReturnNotFound(context));
app.MapMethods("/product/{id}", new[] {"GET","POST"}, async context => ReturnNotFound(context));
app.MapMethods("/product", new[] {"GET", "PATCH", "DELETE"}, async context => ReturnNotFound(context));

app.MapWhen(x => x.Response.StatusCode.Equals(404), applicationBuilder => applicationBuilder.Run(async context => ReturnNotFound(context)));

app.Run();

async void ReturnNotFound(HttpContext context)
{
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = 404;
    const string response = @"{ ""error"": { ""code"": 404, ""message"": ""Not found"" } }";
    await context.Response.WriteAsync(response);
}