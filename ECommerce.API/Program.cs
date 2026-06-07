using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ECommerce.DAL.Context;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Repositories.Generic;
using ECommerce.DAL.Repositories;
using ECommerce.DAL.Repositories.Interfaces;
using ECommerce.DAL.UnitOfWork;
using ECommerce.BLL.Services;
using ECommerce.BLL.Interfaces;
using ECommerce.BLL.Validators;
using FluentValidation;
using ECommerce.API.Extensions;
using ECommerce.API.Middleware;
using Microsoft.AspNetCore.Mvc;

/*
 * ================================================================================
 * Program.cs – Main Entry Point
 * ================================================================================
 * 
 * الشرح:
 * 1. نبني الـ Host ونضيف كل الـ Services المطلوبة (DI Container)
 * 2. نضيف CORS، JWT، Identity
 * 3. نضيف كل الـ Services و Repositories
 * 4. نضيف FluentValidation
 * 5. نضيف AutoMapper
 * 6. نضيف Swagger
 * 7. نعمل Migration للـ Database لو محتاج
 * 8. نشغل الـ Pipeline
 * 
 * ================================================================================
 */

var builder = WebApplication.CreateBuilder(args);

// ========================
// 1. Database Configuration
// ========================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ASPNETCoreD11"),
        sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// ========================
// 2. Identity Configuration
// ========================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ========================
// 3. JWT Authentication
// ========================
builder.Services.AddJwtAuthentication(builder.Configuration);

// ========================
// 4. Policy-Based Authorization
// ========================
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("UserOnly", policy => policy.RequireRole("User", "Admin"))
    .AddPolicy("AdminOrManager", policy => policy.RequireRole("Admin", "Manager"));

// ========================
// 5. Repositories & Services Registration
// ========================
// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Non-Generic Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Image Service – uses a configurable upload path
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
builder.Services.AddSingleton<IImageService>(new ImageService(uploadPath));

// ========================
// 6. AutoMapper
// ========================
builder.Services.AddAutoMapper(typeof(ECommerce.BLL.Mapping.MappingProfile));

// ========================
// 7. FluentValidation
// ========================
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ========================
// 8. Controllers & API Config
// ========================
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ECommerce.API.Filters.ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic 400 responses so we handle validation ourselves via FluentValidation
        options.SuppressModelStateInvalidFilter = true;
    });

// ========================
// 9. CORS
// ========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========================
// 10. Scalar OpenAPI (for API documentation)
// ========================
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "E-Commerce API",
            Version = "v1",
            Description = "Premium E-Commerce APIs for product browsing, cart management, and ordering."
        };

        var securityScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT Bearer token only (do not prefix with 'Bearer ')"
        };

        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", securityScheme);

        var securityRequirement = new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        };

        document.SecurityRequirements.Add(securityRequirement);
        return Task.CompletedTask;
    });
});

// ========================
// Build the App
// ========================
var app = builder.Build();

// ========================
// Seed default roles and Admin user on startup
// ========================
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.SeedDataAsync();
}

// ========================
// Middleware Pipeline
// ========================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("E-Commerce API Docs")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();      // Serve static files (category & product images)
app.UseCors("AllowAll");
app.UseAuthentication();   // JWT Authentication Middleware
app.UseAuthorization();    // Policy-Based Authorization Middleware

// Custom Exception Middleware (global error handler)
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();