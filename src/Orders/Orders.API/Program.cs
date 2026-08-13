using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Orders.API.Data;
using Orders.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- CQRS / MediatR / Carter (idéntico patrón a Catalog.API y Basket.API) ----
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// ---- MongoDB Atlas ----
builder.Services.Configure<MongoOrdersSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IOrdersRepository, OrdersRepository>();

// ---- Cliente HTTP hacia Basket.API ----
builder.Services.AddHttpClient<IBasketApiClient, BasketApiClient>(client =>
{
    var basketApiBaseUrl = builder.Configuration["Services:BasketApiBaseUrl"]
        ?? throw new InvalidOperationException("Falta configurar Services:BasketApiBaseUrl");
    client.BaseAddress = new Uri(basketApiBaseUrl);
});

// ---- CORS: mismo criterio que Catalog.API/Basket.API ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://tranquil-pudding-2d75ce.netlify.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---- Manejo de errores centralizado (reutiliza BuildingBlocks tal cual) ----
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

// ---- Swagger / OpenAPI (requisito del examen) ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders.API",
        Version = "v1",
        Description = "Microservicio de Órdenes de Compra - Examen Práctico de Integración de Microservicios (UTTT)"
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders.API v1");
});

app.MapCarter();
app.UseExceptionHandler(options => { });

app.Run();
