using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Pfm.Api.ErrorHandling;
using Pfm.Application;
using Pfm.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Keeps deserializer messages (which contain internal type names) out of the response.
        options.AllowInputFormatterExceptionMessages = false;
    });

// The OpenAPI document generator reads the minimal-API JSON options, not the MVC ones, so the
// enum converter has to be registered here as well for enums to be documented as strings.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = context =>
        ValidationProblemFactory.Create(context.HttpContext, context.ModelState));

builder.Services.AddCors(options => options.AddPolicy(frontendCorsPolicy, policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

builder.Services.AddOpenApi(options => options.AddSchemaTransformer((schema, context, _) =>
{
    // The default is "double", which contradicts how money is actually carried and stored.
    if (context.JsonTypeInfo.Type == typeof(decimal) || context.JsonTypeInfo.Type == typeof(decimal?))
    {
        schema.Format = "decimal";
    }

    return Task.CompletedTask;
}));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PFM API v1"));
}

app.UseCors(frontendCorsPolicy);

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
