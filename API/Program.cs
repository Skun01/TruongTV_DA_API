using System.Text.Json.Serialization;
using API.Extensions;
using Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", 
    policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // cho phép convert enum sang string và ngược lại
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOptionSettingsExtension(builder.Configuration);
builder.Services.AddAuthConfigurationExtension(builder.Configuration);
builder.Services.AddSwaggerExtension();
var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tài liệu API");
        c.RoutePrefix = string.Empty;
    });    
}
app.MapControllers();

app.Run();
