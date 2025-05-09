using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Tutorial8.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication();
builder.Services.AddControllers();

builder.Services.AddScoped<ITripsService, TripsService>();

//część wykładowa by móc w swaggerze zobaczyć czy mamy zaimplementowane końcówki
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Management API",
        Version = "v1",
        Description = "REST API for managing orders.",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com",
            Url = new Uri("https://www.example.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//również część wykładowa
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Management API v1");
    
    //Basic UI Customization
    c.DocExpansion(DocExpansion.List);
    c.DefaultModelExpandDepth(0); //Hide schemas section by default
    c.DisplayRequestDuration(); // Show request duration
    c.EnableFilter(); // Enable filtering operations
});

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();