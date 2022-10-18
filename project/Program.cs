using JustBuyApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProjectContext>(options =>
    options
        .UseLazyLoadingProxies()
        .UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.Parse("8.0.30-mysql") 
    ));
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
        options.Events = new JwtBearerEvents();
        options.Events.OnChallenge = context =>
        {
            // Перехват ошибки авторизации и возврат сообщения вместо 401
            context.HandleResponse();
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            const string error = @"{ ""error"": { ""code"": 403, ""message"": ""Login failed"" } }";
            return context.Response.WriteAsync(error);
        };
        options.Events.OnForbidden = context =>
        {
            // Перехват ошибки доступа и возврат сообщения вместо 403
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            const string error = @"{ ""error"": { ""code"": 403, ""message"": ""Forbidden for you"" } }";
            return context.Response.WriteAsync(error);
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
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
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHsts();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();